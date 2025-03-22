$ErrorActionPreference = "Stop";
$ProgressPreference = "SilentlyContinue"

# Set the root of the repository
$repoRoot = Resolve-Path "$PSScriptRoot/."

$tld = "sugcon2025otel.localhost"

# Adding initial env files
"NEW_RELIC_API_KEY=" | Out-File (Join-Path $repoRoot ".\apps\otel-collector\.env")
"AZURE_SERVICEBUS_CONNECTIONSTRING=" | Out-File (Join-Path $repoRoot ".\apps\dotnet-listener\.env")

Write-Host "Preparing your Sitecore Containers environment!" -ForegroundColor Green

################################################
# Retrieve and import SitecoreDockerTools module
################################################

# Check for Sitecore Gallery
Import-Module PowerShellGet
$SitecoreGallery = Get-PSRepository | Where-Object { $_.SourceLocation -eq "https://nuget.sitecore.com/resources/v2" }
if (-not $SitecoreGallery)
{
    Write-Host "Adding Sitecore PowerShell Gallery..." -ForegroundColor Green
    Unregister-PSRepository -Name SitecoreGallery -ErrorAction SilentlyContinue
    Register-PSRepository -Name SitecoreGallery -SourceLocation "https://nuget.sitecore.com/resources/v2" -InstallationPolicy Trusted
    $SitecoreGallery = Get-PSRepository -Name SitecoreGallery
}

# Install and Import SitecoreDockerTools
$dockerToolsVersion = "10.2.7"
Remove-Module SitecoreDockerTools -ErrorAction SilentlyContinue
if (-not (Get-InstalledModule -Name SitecoreDockerTools -RequiredVersion $dockerToolsVersion -ErrorAction SilentlyContinue))
{
    Write-Host "Installing SitecoreDockerTools..." -ForegroundColor Green
    Install-Module SitecoreDockerTools -RequiredVersion $dockerToolsVersion -Scope CurrentUser -Repository $SitecoreGallery.Name
}
Write-Host "Importing SitecoreDockerTools..." -ForegroundColor Green
Import-Module SitecoreDockerTools -RequiredVersion $dockerToolsVersion
Write-SitecoreDockerWelcome

##################################
# Configure TLS/HTTPS certificates
##################################

Push-Location (Join-Path $repoRoot "\docker\data\traefik\certs")
try
{
    $mkcert = ".\mkcert.exe"
    if ($null -ne (Get-Command mkcert.exe -ErrorAction SilentlyContinue))
    {
        # mkcert installed in PATH
        $mkcert = "mkcert"
    }
    elseif (-not (Test-Path $mkcert))
    {
        Write-Host "Downloading and installing mkcert certificate tool..." -ForegroundColor Green
        Invoke-WebRequest "https://github.com/FiloSottile/mkcert/releases/download/v1.4.4/mkcert-v1.4.4-windows-amd64.exe" -UseBasicParsing -OutFile mkcert.exe
        if ((Get-FileHash mkcert.exe).Hash -ne "D2660B50A9ED59EADA480750561C96ABC2ED4C9A38C6A24D93E30E0977631398")
        {
            Remove-Item mkcert.exe -Force
            throw "Invalid mkcert.exe file"
        }
    }
    Write-Host "Generating Traefik TLS certificate..." -ForegroundColor Green
    & $mkcert -install
    & $mkcert "*.$tld"

}
catch
{
    Write-Error "An error occurred while attempting to generate TLS certificate: $_"
}
finally
{
    Pop-Location
}

################################
# Add Windows hosts file entries
################################

Write-Host "Adding Windows hosts file entries..." -ForegroundColor Green

Add-HostsEntry "cm-sc-platform.$tld"
Add-HostsEntry "id-sc-platform.$tld"
Add-HostsEntry "solr-sc-platform.$tld"
Add-HostsEntry "aspire-dashboard.$tld"
Add-HostsEntry "dotnet-api.$tld"
Add-HostsEntry "nextjs-head.$tld"

Write-Host "Done!" -ForegroundColor Green
