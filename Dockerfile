FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o app/published_code

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /app/published_code .

ENTRYPOINT ["dotnet", "MyApp.dll"]