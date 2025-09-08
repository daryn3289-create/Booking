FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/IdentityService/Identity.Api/Identity.Api.csproj", "services/IdentityService/Identity.Api/"]
COPY ["services/IdentityService/Identity.Application/Identity.Application.csproj", "services/IdentityService/Identity.Application/"]
COPY ["Booking.Shared/Booking.Shared.csproj", "Booking.Shared/"]
COPY ["services/IdentityService/Identity.Client/Identity.Client.csproj", "services/IdentityService/Identity.Client/"]
COPY ["services/IdentityService/Identity.Infrastructure/Identity.Infrastructure.csproj", "services/IdentityService/Identity.Infrastructure/"]
RUN dotnet restore "services/IdentityService/Identity.Api/Identity.Api.csproj"
COPY . .
WORKDIR "/src/services/IdentityService/Identity.Api"
RUN dotnet build "./Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
