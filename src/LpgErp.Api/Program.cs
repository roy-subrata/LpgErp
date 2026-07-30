using System.Text;
using System.Threading.RateLimiting;
using LpgErp.Api.Authorization;
using LpgErp.Api.Filters;
using LpgErp.Api.Middleware;
using LpgErp.Infrastructure;
using LpgErp.Infrastructure.Auth;
using LpgErp.Infrastructure.Hubs;
using LpgErp.Infrastructure.Persistence;
using LpgErp.Application;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/lpg-erp-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    builder.Host.UseSerilog();

    // Validation runs for every action argument that has a registered validator — see ValidationFilter.
    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            // Allow any localhost origin in dev so the Angular dev server works on
            // whatever port it lands on (4200, or an alternate if that's taken).
            policy.SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                    return uri.IsLoopback;
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Without this the limiter returns an empty 429 body, so the client can't tell
        // "too many attempts" apart from "wrong credentials".
        options.OnRejected = async (context, ct) =>
        {
            var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var window)
                ? (int)Math.Ceiling(window.TotalSeconds)
                : 60;

            context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
            context.HttpContext.Response.ContentType = "application/json";

            Log.Warning("Rate limit hit on {Path} from {Ip}",
                context.HttpContext.Request.Path,
                context.HttpContext.Connection.RemoteIpAddress?.ToString());

            await context.HttpContext.Response.WriteAsJsonAsync(
                ApiResponse.Fail($"Too many attempts. Please try again in {retryAfter} second{(retryAfter == 1 ? "" : "s")}."),
                ct);
        };

        options.AddPolicy("auth-strict", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetTokenBucketLimiter($"auth:{ip}", _ =>
                new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 5,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    TokensPerPeriod = 5,
                    AutoReplenishment = true,
                    QueueLimit = 0
                });
        });

        options.AddPolicy("api-general", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetTokenBucketLimiter($"api:{ip}", _ =>
                new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 100,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    TokensPerPeriod = 100,
                    AutoReplenishment = true,
                    QueueLimit = 0
                });
        });
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection("Jwt").Bind(jwtSettings);

    // The SecretKey in appsettings.json is a placeholder so local dev runs out of the box.
    // Every deployed environment must supply its own via the Jwt__SecretKey env var — a shared
    // or committed key lets anyone forge tokens, and lets a test token authenticate against prod.
    if (!builder.Environment.IsDevelopment())
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
            jwtSettings.SecretKey.StartsWith("CHANGE_ME", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Jwt__SecretKey is not configured. Set a unique per-environment key " +
                "(see deployments/.env.example) before starting outside Development.");
        }

        if (Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt__SecretKey must be at least 32 bytes to sign HMAC-SHA256 tokens.");
        }
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
    builder.Services.AddAuthorization(options =>
    {
        // Receiving/delivering/closing are workflow transitions on a resource someone can already
        // Edit — holding Edit is enough to do them too, so these three policies accept either
        // permission rather than just the one matching the policy's own name.
        var impliedByEdit = new Dictionary<string, string>
        {
            [AppPermissions.PurchaseOrders.Receive] = AppPermissions.PurchaseOrders.Edit,
            [AppPermissions.SalesOrders.Deliver] = AppPermissions.SalesOrders.Edit,
            [AppPermissions.VehicleLoading.Close] = AppPermissions.VehicleLoading.Edit,
        };

        // One policy per known permission, named after the permission string itself, so an action
        // just writes [Authorize(Policy = AppPermissions.Customers.View)] — the compiler catches a
        // typo'd permission name the way a raw string never would.
        foreach (var permission in AppPermissions.GetAll())
        {
            var anyOf = impliedByEdit.TryGetValue(permission, out var editEquivalent)
                ? [permission, editEquivalent]
                : new[] { permission };

            options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(anyOf)));
        }
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<LpgErpDbContext>();
        try
        {
            db.Database.Migrate();

            // Seed demo data in Development, or anywhere SeedData=true (e.g. the docker test stack).
            if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("SeedData"))
            {
                await DbSeeder.SeedAsync(db);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not apply database migrations or seed data");
        }
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Behind the nginx reverse proxy the container only speaks HTTP; TLS terminates at the proxy.
    if (app.Configuration.GetValue("UseHttpsRedirect", true))
    {
        app.UseHttpsRedirection();
    }
    app.UseCors("AllowAngular");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    // Non-zero exit so a misconfigured container shows as failed instead of "exited (0)".
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
