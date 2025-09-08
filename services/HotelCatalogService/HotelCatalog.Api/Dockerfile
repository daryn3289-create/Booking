FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/HotelCatalogService/HotelCatalog.Api/HotelCatalog.Api.csproj", "services/HotelCatalogService/HotelCatalog.Api/"]
COPY ["Booking.Shared/Booking.Shared.csproj", "Booking.Shared/"]
COPY ["services/HotelCatalogService/HotelCatalog.Application/HotelCatalog.Application.csproj", "services/HotelCatalogService/HotelCatalog.Application/"]
COPY ["services/HotelCatalogService/HotelCatalog.Infrastructure/HotelCatalog.Infrastructure.csproj", "services/HotelCatalogService/HotelCatalog.Infrastructure/"]
COPY ["services/HotelCatalogService/HotelCatalog.Domain/HotelCatalog.Domain.csproj", "services/HotelCatalogService/HotelCatalog.Domain/"]
RUN dotnet restore "services/HotelCatalogService/HotelCatalog.Api/HotelCatalog.Api.csproj"
COPY . .
WORKDIR "/src/services/HotelCatalogService/HotelCatalog.Api"
RUN dotnet build "./HotelCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HotelCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HotelCatalog.Api.dll"]
