# Use the appropriate Windows-based images
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN mkdir uploads
EXPOSE 8080
EXPOSE 8081
 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BusinessAppDeploy/BusinessAppDeploy.csproj", "BusinessAppDeploy/"]
RUN dotnet restore "BusinessAppDeploy/BusinessAppDeploy.csproj"
COPY . .
WORKDIR "/src/BusinessAppDeploy"
RUN dotnet build "BusinessAppDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/build
 
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BusinessAppDeploy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BusinessAppDeploy.dll"]