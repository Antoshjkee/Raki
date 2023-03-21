#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Raki.TelegramBot.API/Raki.TelegramBot.API.csproj", "src/Raki.TelegramBot.API/"]
RUN dotnet restore "src/Raki.TelegramBot.API/Raki.TelegramBot.API.csproj"
COPY . .
WORKDIR "/src/src/Raki.TelegramBot.API"
RUN dotnet build "Raki.TelegramBot.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Raki.TelegramBot.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Raki.TelegramBot.API.dll"]