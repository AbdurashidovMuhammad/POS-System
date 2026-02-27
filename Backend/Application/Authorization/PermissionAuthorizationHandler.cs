using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // SuperAdmin always passes
        if (context.User.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // For other roles, check JWT "perm" claim
        var requiredClaim = $"{requirement.Section}.{requirement.Action}";
        var hasPerm = context.User.Claims
            .Any(c => c.Type == "perm" && c.Value == requiredClaim);

        if (hasPerm)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
