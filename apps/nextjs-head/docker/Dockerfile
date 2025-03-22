#escape=`
ARG BUILD_IMAGE=mcr.microsoft.com/windows/servercore:ltsc2022
ARG BASE_IMAGE=mcr.microsoft.com/windows/servercore:ltsc2022

FROM ${BUILD_IMAGE} AS download
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ARG NODEJS_VERSION=v23.6.1
RUN $Uri = 'https://nodejs.org/dist/{0}/node-{0}-win-x64.zip' -f $env:NODEJS_VERSION; `
    $OutFile = 'c:/node-v{0}-win-x64.zip' -f $env:NODEJS_VERSION ; `
    Invoke-WebRequest -Uri $Uri -OutFile $OutFile -Verbose;  `
    Expand-Archive $OutFile -DestinationPath c:/ ; `
    Copy-Item $('c:/node-{0}-win-x64' -f $env:NODEJS_VERSION) -Destination c:/out/nodejs -Recurse ; `
    Remove-Item $('c:/node-{0}-win-x64' -f $env:NODEJS_VERSION) -Recurse -Force ;


FROM ${BASE_IMAGE} AS nodejs

RUN md %APPDATA%\npm
ENV PATH=C:\Windows\system32;C:\Windows;c:\nodejs;%APPDATA%\npm 
COPY --from=download /out/ /

COPY *.cmd /

ENV PORT=5000
EXPOSE 5000
ENTRYPOINT ["cmd", "/c", "c:\\run.cmd"]
