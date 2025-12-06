using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using premake;
using System;
using System.IO;
using System.Net.Http;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddMemoryCache()
    //.AddFirebase()
    .AddRazorPages()
    .AddRazorRuntimeCompilation().WithRazorPagesRoot("/src/frontend/Pages");
builder.Services.AddScoped(sp =>
{
    NavigationManager navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
})
;





builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddServerSideBlazor();
builder.Services.AddSwaggerGen(c =>
{
});
var host = builder.Build();

if (host.Environment.IsDevelopment())
{
    host.MapOpenApi();
    host.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "VOID api");
        
        }
    );
}
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
