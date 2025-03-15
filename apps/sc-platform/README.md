# Sitecore Platform

...

## Building

1. `msbuild /v:m /p:Configuration=Debug /t:"Restore;Build" /p:DeployOnBuild=true /p:PublishProfile=DockerPublish` or publish `Platform` project from Visual Studio.
