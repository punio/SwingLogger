FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["SwingDataViewer/SwingDataViewer.csproj", "SwingDataViewer/"]
COPY ["../Common/SwingCommon.csproj", "../Common/"]
RUN dotnet restore "SwingDataViewer/SwingDataViewer.csproj"
COPY . .
WORKDIR "/src/SwingDataViewer"
RUN dotnet build "SwingDataViewer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SwingDataViewer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SwingDataViewer.dll"]