FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5050

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Glense.Server.csproj", "./"]
RUN dotnet restore "Glense.Server.csproj"
COPY . .
RUN dotnet build "Glense.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Glense.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --chown=app:app --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "Glense.Server.dll"]
