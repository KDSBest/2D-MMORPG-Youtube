#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ScaleUpProblem/ScaleUpProblem.csproj", "ScaleUpProblem/"]
RUN dotnet restore "ScaleUpProblem/ScaleUpProblem.csproj"
COPY . .
WORKDIR "/src/ScaleUpProblem"
RUN dotnet build "ScaleUpProblem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScaleUpProblem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUpProblem.dll"]