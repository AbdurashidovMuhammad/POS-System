using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Application.Authorization;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    // Policy name format: "Permission:Section:Action"
    private const string PolicyPrefix = "Permission:";

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var parts = policyName[PolicyPrefix.Length..].Split(':');
            if (parts.Length == 2)
            {
                var section = parts[0];
                var action = parts[1];

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(section, action))
                    .Build();

                return policy;
            }
        }

        return await base.GetPolicyAsync(policyName);
    }
}
