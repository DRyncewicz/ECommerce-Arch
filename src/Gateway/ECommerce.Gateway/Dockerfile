FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/Shared/ECommerce.SharedKernel/ECommerce.SharedKernel.csproj src/Shared/ECommerce.SharedKernel/
COPY src/Gateway/ECommerce.Gateway/ECommerce.Gateway.csproj src/Gateway/ECommerce.Gateway/
RUN dotnet restore src/Gateway/ECommerce.Gateway/ECommerce.Gateway.csproj

COPY src/Shared/ECommerce.SharedKernel/ src/Shared/ECommerce.SharedKernel/
COPY src/Gateway/ECommerce.Gateway/ src/Gateway/ECommerce.Gateway/
RUN dotnet publish src/Gateway/ECommerce.Gateway/ECommerce.Gateway.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.Gateway.dll"]
