FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["DC bot/DC bot.csproj", "DC bot/"]
RUN dotnet restore "DC bot/DC bot.csproj"
COPY . .
WORKDIR "/src/DC bot"
RUN dotnet build "DC bot.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "DC bot.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MelodiasMario.dll"]
