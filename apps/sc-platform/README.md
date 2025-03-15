# Sitecore Platform

...

## Prerequisites

- Visual Studio 2022
- Docker Desktop or Docker engine
- Sitecore license file

## Starting

1. `.\init.ps1 -License C:\license\license.xml -AdminPassword b`
1. `dotnet tool restore`
1. `msbuild /v:m /p:Configuration=Debug /t:"Restore;Build" /p:DeployOnBuild=true /p:PublishProfile=DockerPublish` or publish `Platform` project from Visual Studio.
1. `docker compose up -d --build`
1. `dotnet sitecore login --authority https://id.smotel.localhost --cm https://cm-platform.smotel.localhost --allow-write true`
1. `dotnet sitecore index schema-populate`
