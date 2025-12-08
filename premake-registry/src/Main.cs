using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OpenIddict.Client.AspNetCore;
using premake;
using premake.repositories.user;
using premake.User;
using premake_registry.src.frontend.Pages;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddMemoryCache()
    //.AddFirebase()
    .AddRazorPages()
    .AddRazorRuntimeCompilation().WithRazorPagesRoot("/src/frontend/Pages");
/*
builder.Services.AddHttpClient("BlazorClient", (sp, client) =>
{
    var navigation = sp.GetRequiredService<NavigationManager>();
    client.BaseAddress = new Uri(navigation.BaseUri);
});

*/




builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddServerSideBlazor();
builder.Services.AddSwaggerGen(c =>
{
});



builder.Services.AddAuthentication(options =>
{
    // Default for local persistence
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // Default for challenges (login)
    options.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseInMemoryDatabase("db");
    options.UseOpenIddict();
});
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        options.UseEntityFrameworkCore()
               .UseDbContext<DbContext>();
    })
    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Allow the OpenIddict client to negotiate the authorization code flow.
        options.AllowAuthorizationCodeFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableRedirectionEndpointPassthrough().DisableTransportSecurityRequirement();

        // Register the GitHub integration.
        options.UseWebProviders()
               .AddGitHub(options =>
               {
                   options.SetClientId(premake.Config.GetGithubClientId())
                          .SetClientSecret(premake.Config.GetGithubClientSecret())
                          .SetRedirectUri("Auth/callback");
               });
        options.UseSystemNetHttp();

        options.AddRegistration(new OpenIddict.Client.OpenIddictClientRegistration
        {
            Issuer = new Uri("https://github.com/"),
            ClientId = premake.Config.GetGithubClientId(),
            ClientSecret = premake.Config.GetGithubClientSecret(),
            Scopes = { "read:user" },

        });

    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<GitRepoRepository>();

var host = builder.Build();

if (host.Environment.IsDevelopment())
{
    host.MapOpenApi();
    host.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "VOID api");
        
        }
    );
}


host.UseAuthentication();
host.UseAuthorization();
host.MapControllers();
host.MapRazorPages();
host.UseRouting();
host.MapBlazorHub();
host.MapFallbackToPage("/_Host");
if (host.Environment.IsDevelopment())
{
    host.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "obj", "Debug", "net9.0", "win-x64", "scopedcss", "bundle")),
        RequestPath = "/css"
    });
} else
{
    host.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
          Path.Combine(host.Environment.ContentRootPath, "wwwroot")),
        RequestPath = "/css"
    });
}
await host.RunAsync();
