# escape=`
ARG TAG=ltsc2022

# ---
FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-$TAG AS build

WORKDIR /source

# copy project file and restore as distinct layers
COPY ./*.sln .
COPY ./src/ListenerApp/*.csproj ./src/ListenerApp/
RUN dotnet restore

# copy source code and publish app
COPY . .
RUN dotnet publish ./src/ListenerApp/ -o /app

# ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0-nanoserver-$TAG

EXPOSE 8080

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["ListenerApp"]
