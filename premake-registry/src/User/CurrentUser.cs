using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
#nullable enable
namespace premake.User
{
    public class CurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal userPrincipal => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(); 
        public string UserName { get => userPrincipal?.GetClaim(ClaimTypes.Name) ?? string.Empty; }
        public string AvatarUri { get => userPrincipal?.GetClaim("avatar") ?? string.Empty; }

        public string ReposUri { get => userPrincipal?.GetClaim("repos") ?? string.Empty; }
        public CurrentUser(IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        public bool IsLoggedIn()
        {
            return userPrincipal.Identity?.IsAuthenticated ?? false;
        }

        private async Task<string?> GetToken()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            return await httpContext.GetTokenAsync("access_token"); ;
        }

        /// <summary>
        /// Generic GET request to an API endpoint, deserializing JSON into TOutput.
        /// Automatically attaches bearer token if available.
        /// </summary>
        public async Task<TOutput?> GetFromApiAsync<TOutput>(string apiUrl)
        {
            // Attach token if present
            if (IsLoggedIn())
            {
                var token = await GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
            // GitHub requires a User-Agent header
            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("premake-registry");
            }

            var response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode == false)
            {
                return default;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<TOutput>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result;
        }
    }
}
