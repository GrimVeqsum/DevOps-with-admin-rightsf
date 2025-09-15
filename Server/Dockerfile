# Используем образ с .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ТОЛЬКО файлы проектов
COPY *.sln .
COPY DinoServer/*.csproj ./DinoServer/
RUN dotnet restore "./DinoServer/DinoServer.csproj"

# Копируем ВСЁ остальное
COPY . .

# Собираем и публикуем
RUN dotnet publish "DinoServer/DinoServer.csproj" -c Release -o /app

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "DinoServer.dll"]

