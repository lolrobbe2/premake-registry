# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source/premake-registry

# Copy everything and build app
COPY premake-registry/. .
# copy everything and restore, build app
RUN dotnet publish -c release -o /app
# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "premake-registry.dll"]
