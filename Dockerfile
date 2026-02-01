FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln .
COPY FitBridge_Domain/*.csproj FitBridge_Domain/
COPY FitBridge_Infrastructure/*.csproj FitBridge_Infrastructure/
COPY FitBridge_Application/*.csproj FitBridge_Application/
COPY FitBridge_API/*.csproj FitBridge_API/
RUN dotnet restore

COPY . .

RUN dotnet publish FitBridge_API/FitBridge_API.csproj \
    -c Release \
    -o /app \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as final
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FitBridge_API.dll"]