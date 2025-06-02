# Этап базового образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем csproj
COPY ShortsPoster.csproj ./
RUN dotnet restore ./ShortsPoster.csproj

# Копируем все остальные файлы проекта
COPY . ./
RUN dotnet build ./ShortsPoster.csproj -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Публикация
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShortsPoster.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortsPoster.dll"]

