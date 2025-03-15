param(
  [Parameter(Mandatory = $false)]
  [switch]$RebuildIndexes
)

$ProgressPreference = "SilentlyContinue"
$ErrorActionPreference = "Stop"

# ensure Windows docker engine is running
docker desktop engine use windows

# ensure images are up to date
Write-Host "Keeping images up to date..." -ForegroundColor Green
(docker compose config | Select-String "(scr\.sitecore\.com\/.+)|(mcr\.microsoft\.com\/.+)|(traefik\:v.+)|(ghcr\.io\/.+)").Matches | Select-Object -Unique | ForEach-Object { $_.Value } | ForEach-Object { docker image pull $_ }

# build stack
Write-Host "Build stack..." -ForegroundColor Green

docker compose build

if ($LASTEXITCODE -ne 0)
{
  Write-Error "Container build failed, see errors above."
}

# start stack
Write-Host "Starting stack..." -ForegroundColor Green

docker compose up -d

# start local XM Cloud instance
Push-Location .\apps\sc-platform

try
{
  .\up.ps1 -RebuildIndexes:$RebuildIndexes
}
finally
{
  Pop-Location
}

# wait for Traefik to expose the head route
# Write-Host "Waiting for head to become available..." -ForegroundColor Green -NoNewline
# $startTime = Get-Date
# do
# {
#   Start-Sleep -Milliseconds 100
#   try
#   {
#     $status = Invoke-RestMethod "http://localhost:8079/api/http/routers/headnextjs@docker"
#   }
#   catch
#   {
#     if ($_.Exception.Response.StatusCode.value__ -ne "404")
#     {
#       throw
#     }
#   }
#   Write-Host "." -ForegroundColor Green -NoNewline
# } while ($status.status -ne "enabled" -and $startTime.AddMinutes(5) -gt (Get-Date))

# Write-Host "`n" -NoNewline

# if (-not $status.status -eq "enabled")
# {
#   $status

#   Write-Error "Timeout waiting for the head to become available via Traefik proxy."
# }


# finish
Write-Host "Done!" -ForegroundColor Green