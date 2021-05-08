#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WorldChatService/WorldChatService.csproj", "WorldChatService/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["UnityAzureNetworkEngine/Core/ReliableUdp/ReliableUdp.csproj", "UnityAzureNetworkEngine/Core/ReliableUdp/"]
COPY ["CommonServer/CommonServer.csproj", "CommonServer/"]
RUN dotnet restore "WorldChatService/WorldChatService.csproj"
COPY . .
WORKDIR "/src/WorldChatService"
RUN dotnet build "WorldChatService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorldChatService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorldChatService.dll"]