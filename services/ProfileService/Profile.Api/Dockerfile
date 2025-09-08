FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/ProfileService/Profile.Api/Profile.Api.csproj", "services/ProfileService/Profile.Api/"]
COPY ["Booking.Shared/Booking.Shared.csproj", "Booking.Shared/"]
COPY ["services/ProfileService/Profile.Infrastructure/Profile.Infrastructure.csproj", "services/ProfileService/Profile.Infrastructure/"]
COPY ["services/ProfileService/Profile.Domain/Profile.Domain.csproj", "services/ProfileService/Profile.Domain/"]
RUN dotnet restore "services/ProfileService/Profile.Api/Profile.Api.csproj"
COPY . .
WORKDIR "/src/services/ProfileService/Profile.Api"
RUN dotnet build "./Profile.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Profile.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Profile.Api.dll"]
