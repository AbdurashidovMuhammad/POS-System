using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string section, string action)
        : base(policy: $"Permission:{section}:{action}")
    {
    }
}
