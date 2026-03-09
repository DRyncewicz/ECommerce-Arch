FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/Shared/ECommerce.SharedKernel/ECommerce.SharedKernel.csproj src/Shared/ECommerce.SharedKernel/
COPY src/Shared/ECommerce.Contracts/ECommerce.Contracts.csproj src/Shared/ECommerce.Contracts/
COPY src/Services/PaymentService/ECommerce.PaymentService/ECommerce.PaymentService.csproj src/Services/PaymentService/ECommerce.PaymentService/
RUN dotnet restore src/Services/PaymentService/ECommerce.PaymentService/ECommerce.PaymentService.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/PaymentService/ECommerce.PaymentService/ src/Services/PaymentService/ECommerce.PaymentService/
RUN dotnet publish src/Services/PaymentService/ECommerce.PaymentService/ECommerce.PaymentService.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.PaymentService.dll"]
