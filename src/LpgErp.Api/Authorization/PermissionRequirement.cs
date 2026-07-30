using Microsoft.AspNetCore.Authorization;

namespace LpgErp.Api.Authorization;

/// <summary>
/// Satisfied if the caller holds any one of the listed permissions. Almost always a single
/// permission — the list only has more than one entry for the handful of workflow-transition
/// actions (receiving goods, delivering a sale, closing a loading) that are also implied by
/// holding the resource's general Edit permission, so an editor doesn't need a second, narrower
/// grant on top of what they already have.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] AnyOf { get; }

    public PermissionRequirement(params string[] anyOf)
    {
        AnyOf = anyOf;
    }
}

/// <summary>
/// Checks the "permission" claims already issued at login (AuthService.GenerateAccessToken) against
/// a requirement's allowed set. Nothing here talks to the database — by design, a permission
/// change only takes effect for a user's next login, the same way role changes already do.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var granted = context.User.FindAll("permission").Select(c => c.Value);
        if (requirement.AnyOf.Any(granted.Contains))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
