using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace premake.controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        public AuthController()
        {
        }
        [HttpGet("challenge")]
        public IResult Get() => Results.Challenge(properties: null, authenticationSchemes: [Providers.GitHub]);

        [HttpGet("callback"),HttpPost("callback")]
        public async Task<IResult> Authenticate()
        {
            // Retrieve the authorization data validated by OpenIddict as part of the callback handling.
            var result = await HttpContext.AuthenticateAsync(Providers.GitHub);

            // Build an identity based on the external claims and that will be used to create the authentication cookie.
            var identity = new ClaimsIdentity(authenticationType: "ExternalLogin");


            // By default, OpenIddict will automatically try to map the email/name and name identifier claims from
            // their standard OpenID Connect or provider-specific equivalent, if available. If needed, additional
            // claims can be resolved from the external identity and copied to the final authentication cookie.
            identity.SetClaim(ClaimTypes.Email, result.Principal!.GetClaim(ClaimTypes.Email))
                    .SetClaim(ClaimTypes.Name, result.Principal!.GetClaim("login"))
                    .SetClaim(ClaimTypes.NameIdentifier, result.Principal!.GetClaim(ClaimTypes.NameIdentifier));

            // Preserve the registration details to be able to resolve them later.
            identity.SetClaim(Claims.Private.RegistrationId, result.Principal!.GetClaim(Claims.Private.RegistrationId))
                    .SetClaim(Claims.Private.ProviderName, result.Principal!.GetClaim(Claims.Private.ProviderName));

            // Build the authentication properties based on the properties that were added when the challenge was triggered.
            var properties = new AuthenticationProperties(result.Properties.Items)
            {
                RedirectUri = "/",

            };

            // Ask the default sign-in handler to return a new cookie and redirect the
            // user agent to the return URL stored in the authentication properties.
            //
            // For scenarios where the default sign-in handler configured in the ASP.NET Core
            // authentication options shouldn't be used, a specific scheme can be specified here.
            return Results.SignIn(new ClaimsPrincipal(identity), properties);
        }
        [HttpGet("whoami")]
        public async Task<IResult> WhoAmi()
        {
            var result = await HttpContext.AuthenticateAsync();
            if (result is not { Succeeded: true })
            {
                return Results.Text("You're not logged in.");
            }

            return Results.Text(string.Format("You are {0}.", result.Principal.FindFirst(ClaimTypes.Name)!.Value));
        

        }
}

}