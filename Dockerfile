FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["StarItAll/StarItAll.csproj", "StarItAll/"]
RUN dotnet restore "StarItAll/StarItAll.csproj"
COPY . .
WORKDIR "/src/StarItAll"
RUN dotnet build "StarItAll.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StarItAll.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StarItAll.dll"]
