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
using premake.repositories.registry;
using premake.repositories.user;
using premake.User;
using premake_registry.src.frontend.Pages;
using src.utils;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Clear defaults so all proxies/networks are trusted
    options.KnownProxies.Add(IPAddress.Parse("127.0.10.1"));
});
builder.Services
    .AddMemoryCache()
    .AddFirebase()
    .AddRazorPages()
    .AddRazorRuntimeCompilation().WithRazorPagesRoot("/src/frontend/Pages");





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
               .EnableRedirectionEndpointPassthrough();

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
builder.Services.AddScoped<UserRepositories>();
builder.Services.AddSingleton<IndexRepositories>();
var host = builder.Build();

if (host.Environment.IsDevelopment())
{
    host.MapOpenApi();
    host.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "premake-registry api");
        
        }
    );
}

host.UseHsts();
host.UseForwardedHeaders();
host.UseAuthentication();
host.UseAuthorization();
host.MapControllers();
host.MapRazorPages();
host.UseStaticFiles();
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
host.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next(context);
});
await host.RunAsync();
