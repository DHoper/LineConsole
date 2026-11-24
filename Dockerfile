# LineConsole：一個映像檔裡有前端（React，建置成靜態檔）和後端（ASP.NET Core），同一個網址提供。
# 預設是展示模式：SQLite 記憶體資料庫＋模擬的 LINE＋展示資料，不需要任何帳號。

# ---- 1. 前端：Vite 建置到 /src/LineConsole.Server/wwwroot ----
FROM node:22-alpine AS web
WORKDIR /src/lineconsole.client
COPY lineconsole.client/package.json lineconsole.client/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY lineconsole.client/ ./
RUN npm run build

# ---- 2. 後端：先只複製專案檔還原套件（套件沒變就用快取），再複製原始碼發佈 ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY LineConsole.Domain/LineConsole.Domain.csproj LineConsole.Domain/
COPY LineConsole.Application/LineConsole.Application.csproj LineConsole.Application/
COPY LineConsole.Infrastructure/LineConsole.Infrastructure.csproj LineConsole.Infrastructure/
COPY LineConsole.Server/LineConsole.Server.csproj LineConsole.Server/
RUN dotnet restore LineConsole.Server/LineConsole.Server.csproj
COPY LineConsole.Domain/ LineConsole.Domain/
COPY LineConsole.Application/ LineConsole.Application/
COPY LineConsole.Infrastructure/ LineConsole.Infrastructure/
COPY LineConsole.Server/ LineConsole.Server/
COPY --from=web /src/LineConsole.Server/wwwroot LineConsole.Server/wwwroot
RUN dotnet publish LineConsole.Server/LineConsole.Server.csproj -c Release -o /app --no-restore -p:SkipSpaBuild=true

# ---- 3. 執行：只有 ASP.NET Core 執行環境，以非 root 使用者執行 ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./
RUN mkdir -p /app/log && chown "$APP_UID" /app/log
ENV ASPNETCORE_HTTP_PORTS=8080 \
    LineConsole__Mode=Demo
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "LineConsole.Server.dll"]
