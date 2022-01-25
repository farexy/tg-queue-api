#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TG.Queue.Api/TG.Queue.Api.csproj", "TG.Queue.Api/"]
COPY nuget.config .
RUN dotnet restore "TG.Queue.Api/TG.Queue.Api.csproj"
COPY . .
WORKDIR "/src/TG.Queue.Api"
RUN dotnet build "TG.Queue.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TG.Queue.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TG.Queue.Api.dll"]