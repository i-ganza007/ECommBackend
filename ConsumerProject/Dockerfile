FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY ConsumerProject/ConsumerProject.csproj ConsumerProject/
COPY ECommBackend.ServiceDefaults/ECommBackend.ServiceDefaults.csproj ECommBackend.ServiceDefaults/
RUN dotnet restore ConsumerProject/ConsumerProject.csproj
COPY ConsumerProject/ ConsumerProject/
COPY ECommBackend.ServiceDefaults/ ECommBackend.ServiceDefaults/
RUN dotnet publish ConsumerProject/ConsumerProject.csproj -c Release -o /app/published_code --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /app/published_code .

ARG APP_VERSION=unknown
ENV APP_VERSION=${APP_VERSION}

ENTRYPOINT ["dotnet", "ConsumerProject.dll"]
