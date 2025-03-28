# escape=`

ARG PARENT_IMAGE
ARG TOOLS_IMAGE
ARG MANAGEMENT_SERVICES_IMAGE
ARG HEADLESS_SERVICES_IMAGE
ARG SPE_SERVICES_IMAGE
ARG SXA_SERVICES_IMAGE

FROM ${TOOLS_IMAGE} as tools
FROM ${MANAGEMENT_SERVICES_IMAGE} AS management_services
FROM ${HEADLESS_SERVICES_IMAGE} AS headless_services
FROM ${SPE_SERVICES_IMAGE} AS spe_services
FROM ${SXA_SERVICES_IMAGE} AS sxa_services

# ---
FROM ${PARENT_IMAGE} AS downloads
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/out/downloads
RUN curl.exe -sS -L -o '.\urlrewrite.msi' https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi;

# ---
FROM ${PARENT_IMAGE}
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ENV Sitecore_AppSettings_exmEnabled:define=no `
    Sitecore_GraphQL_Enabled=true `
    sxaxm:define=sxaxmonly

# enable Fusion logging to assist with assembly bind failures
RUN Set-ItemProperty -Path HKLM:\Software\Microsoft\Fusion -Name ForceLog -Value 1 -Type DWord;`
    Set-ItemProperty -Path HKLM:\Software\Microsoft\Fusion -Name LogFailures -Value 1 -Type DWord;`
    Set-ItemProperty -Path HKLM:\Software\Microsoft\Fusion -Name LogResourceBinds -Value 1 -Type DWord;`
    Set-ItemProperty -Path HKLM:\Software\Microsoft\Fusion -Name LogPath -Value 'C:\FusionLog\' -Type String;`
    MKDIR 'C:\FusionLog';

WORKDIR C:\inetpub\wwwroot

# install IIS UrlRewrite
COPY --from=downloads C:/out/downloads/ C:/downloads
RUN Start-Process -Wait -FilePath msiexec -ArgumentList '/i', 'C:\\downloads\\urlrewrite.msi', '/quiet', '/norestart' -NoNewWindow; `
    Remove-Item 'C:\\downloads' -Force -Recurse

# copy developer tools and entrypoints
COPY --from=tools C:\tools C:\tools

# copy and init modules
COPY --from=headless_services C:\module\tools C:\module\tools\headless
COPY --from=headless_services C:\module\cm\content C:\inetpub\wwwroot
COPY --from=management_services C:\module\cm\content C:\inetpub\wwwroot
COPY --from=spe_services C:\module\cm\content C:\inetpub\wwwroot
COPY --from=sxa_services C:\module\tools C:\module\tools\sxa
COPY --from=sxa_services C:\module\cm\content C:\inetpub\wwwroot
RUN C:\module\tools\headless\Initialize-Content.ps1 -TargetPath C:\inetpub\wwwroot; `
    C:\module\tools\sxa\Initialize-Content.ps1 -TargetPath C:\inetpub\wwwroot; `
    Remove-Item -Path C:\module -Recurse -Force;

# copy published web project
COPY . .

# apply transformations
RUN & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\Web.config' -XdtPath 'C:\\inetpub\\wwwroot\\Web.config.Common.xdt'; `
    & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\Web.config' -XdtPath 'C:\\inetpub\\wwwroot\\Web.config.CM.xdt'; `
    & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\Web.config' -XdtPath 'C:\\inetpub\\wwwroot\\Web.config.SitecoreMvcOtel.xdt'; `
    Remove-Item -Path 'C:\\inetpub\\wwwroot\\*.xdt' -Exclude "*.deploy.xdt" -Force;

# delete files in packages folder so it can be mounted
RUN Remove-Item -Path 'C:\inetpub\wwwroot\App_Data\packages\*'