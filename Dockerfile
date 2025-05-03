FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY EasyContinuity-API/*.csproj ./EasyContinuity-API/
COPY EasyContinuity-API.Tests/*.csproj ./EasyContinuity-API.Tests/

RUN dotnet restore

COPY EasyContinuity-API/. ./EasyContinuity-API/
COPY EasyContinuity-API.Tests/. ./EasyContinuity-API.Tests/

WORKDIR /app/EasyContinuity-API
RUN dotnet publish -c Debug -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app/out .

ARG ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "EasyContinuity-API.dll"]