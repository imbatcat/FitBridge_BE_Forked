# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln .
COPY BenchmarkSuite1/*.csproj BenchmarkSuite1/
COPY FitBridge_Domain/*.csproj FitBridge_Domain/
COPY FitBridge_Infrastructure/*.csproj FitBridge_Infrastructure/
COPY FitBridge_Application/*.csproj FitBridge_Application/
COPY FitBridge_API/*.csproj FitBridge_API/
COPY FitBridge_UnitTest/*.csproj FitBridge_UnitTest/

ENV NUGET_PACKAGES=/home/jenkins/.nuget

RUN --mount=type=cache,id=nuget,target=/home/jenkins/.nuget \
    dotnet restore \
    --configuration Release

COPY . .

RUN --mount=type=cache,id=nuget,target=/home/jenkins/.nuget \
    dotnet publish FitBridge_API/FitBridge_API.csproj \
    --property:WarningLevel=0 \
    --configuration Release \
    --output /app \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as final
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FitBridge_API.dll"]