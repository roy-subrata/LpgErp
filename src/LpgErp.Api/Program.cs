using LpgErp.Api.Middleware;
using LpgErp.Infrastructure;
using LpgErp.Infrastructure.Persistence;
using LpgErp.Application;
using Microsoft.EntityFrameworkCore;
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

    builder.Services.AddControllers();
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
                .AllowAnyMethod();
        });
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

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
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
