require "vstudio"
function platformsElement(cfg)
   _p(2, '<Platforms>AnyCpu;arm64;x64</Platforms>')
end

premake.override(premake.vstudio.cs2005.elements, "projectProperties", function (oldfn, cfg)
   return table.join(oldfn(cfg), {
   platformsElement,
   })
end)

workspace "premake-registry"
architecture "x86_64"
   configurations { "Debug", "Release" }
   startproject "premake-registry"

   project "premake-registry"
      kind "ConsoleApp" -- CLI application
      dotnetframework "net9.0" -- Targeting .NET 9.0
      location "premake-registry"
      dotnetsdk "Web"
      language "C#"
      targetdir "bin/%{cfg.buildcfg}"
      files { "%{prj.name}/src/**.cs", "%{prj.name}/src/**.env" }       -- Include all C# source files
      nuget { "Blazor.Bootstrap:3.5.0","DotNetEnv:3.1.1","Google.Cloud.Firestore:3.10.0", "Microsoft.AspNetCore.OpenApi:9.0.8", "Swashbuckle.AspNetCore:9.0.4", "Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation: 9.0.8", "ClosedXML:0.105.0", "Microsoft.EntityFrameworkCore.InMemory:9.0.0", "OpenIddict.AspNetCore:7.2.0","OpenIddict.EntityFrameworkCore:7.2.0","OpenIddict.Client.WebIntegration:7.2.0","OpenIddict.Client.SystemNetHttp:7.2.0","OpenIddict.Validation.SystemNetHttp:7.2.0" }
      vsprops {
         PublishSingleFile = "true",
         SelfContained = "true",
         IncludeNativeLibrariesForSelfExtract = "true"
      }

      filter "configurations:Debug"
         defines { "DEBUG" }
         optimize "Off"
      
      filter "configurations:Release"
         symbols "Off"
         defines { "NDEBUG" }
         optimize "On"
