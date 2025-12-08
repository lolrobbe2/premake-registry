using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using System.Security.Claims;
#nullable enable
namespace premake.User
{
    public class CurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal userPrincipal => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(); 
        public string UserName { get => userPrincipal?.GetClaim(ClaimTypes.Name) ?? "None"; }
        public string AvatarUri { get => userPrincipal?.GetClaim("avatar") ?? "None"; }
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsLoggedIn()
        {
            return userPrincipal.Identity?.IsAuthenticated ?? false;
        }
    }
}
