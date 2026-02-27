using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization;

public record PermissionRequirement(string Section, string Action) : IAuthorizationRequirement;
