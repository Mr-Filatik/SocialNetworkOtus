# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8090
EXPOSE 8091
EXPOSE 8092
EXPOSE 8093
EXPOSE 8094
EXPOSE 8095
EXPOSE 8096


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/DialogApi/DialogApi.csproj", "Applications/Backend/DialogApi/"]
COPY ["Shared/Database/Shared.Database.Entities/Shared.Database.Entities.csproj", "Shared/Database/Shared.Database.Entities/"]
COPY ["Shared/Database/Shared.Database.Tarantool/Shared.Database.Tarantool.csproj", "Shared/Database/Shared.Database.Tarantool/"]
COPY ["Shared/Database/Shared.Database.Abstract/Shared.Database.Abstract.csproj", "Shared/Database/Shared.Database.Abstract/"]
RUN dotnet restore "./Applications/Backend/DialogApi/DialogApi.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/DialogApi"
RUN dotnet build "./DialogApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DialogApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DialogApi.dll"]