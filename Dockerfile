# 使用 .NET SDK 映像作為構建階段
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 複製專案文件
COPY ["CryptoServer.csproj", "./"]

# 還原依賴項
RUN dotnet restore

# 複製所有源代碼
COPY . .

# 構建應用程序
RUN dotnet build -c Release -o /app/build

# 發布應用程序
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# 最終映像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# 安裝 curl
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# 設置環境變量
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

# 暴露端口
EXPOSE 80

ENTRYPOINT ["dotnet", "CryptoServer.dll"] 