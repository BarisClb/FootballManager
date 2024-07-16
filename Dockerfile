FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FootballManager.UI/FootballManager.UI.csproj", "FootballManager.UI/"]
COPY ["FootballManager.Application/FootballManager.Application.csproj", "FootballManager.Application/"]
COPY ["FootballManager.Domain/FootballManager.Domain.csproj", "FootballManager.Domain/"]
COPY ["FootballManager.Infrastructure/FootballManager.Infrastructure.csproj", "FootballManager.Infrastructure/"]
COPY ["FootballManager.Persistence/FootballManager.Persistence.csproj", "FootballManager.Persistence/"]
RUN dotnet restore "FootballManager.UI/FootballManager.UI.csproj"
COPY . .
WORKDIR "/src/FootballManager.UI"
RUN dotnet build "FootballManager.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FootballManager.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FootballManager.UI.dll"]