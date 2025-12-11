FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Volcanion.LedgerService.API/Volcanion.LedgerService.API.csproj", "src/Volcanion.LedgerService.API/"]
COPY ["src/Volcanion.LedgerService.Application/Volcanion.LedgerService.Application.csproj", "src/Volcanion.LedgerService.Application/"]
COPY ["src/Volcanion.LedgerService.Domain/Volcanion.LedgerService.Domain.csproj", "src/Volcanion.LedgerService.Domain/"]
COPY ["src/Volcanion.LedgerService.Infrastructure/Volcanion.LedgerService.Infrastructure.csproj", "src/Volcanion.LedgerService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Volcanion.LedgerService.API/Volcanion.LedgerService.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Volcanion.LedgerService.API"
RUN dotnet build "Volcanion.LedgerService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Volcanion.LedgerService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "Volcanion.LedgerService.API.dll"]
