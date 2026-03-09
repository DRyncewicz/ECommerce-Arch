FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/Shared/ECommerce.SharedKernel/ECommerce.SharedKernel.csproj src/Shared/ECommerce.SharedKernel/
COPY src/Shared/ECommerce.Contracts/ECommerce.Contracts.csproj src/Shared/ECommerce.Contracts/
COPY src/Services/CustomerService/ECommerce.CustomerService/ECommerce.CustomerService.csproj src/Services/CustomerService/ECommerce.CustomerService/
RUN dotnet restore src/Services/CustomerService/ECommerce.CustomerService/ECommerce.CustomerService.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/CustomerService/ECommerce.CustomerService/ src/Services/CustomerService/ECommerce.CustomerService/
RUN dotnet publish src/Services/CustomerService/ECommerce.CustomerService/ECommerce.CustomerService.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.CustomerService.dll"]
