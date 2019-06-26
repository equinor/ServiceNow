FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["servicenowapi/servicenowapi.csproj", "servicenowapi/"]
RUN dotnet restore "servicenowapi/servicenowapi.csproj"
COPY . .
WORKDIR "/src/servicenowapi"
RUN dotnet build "servicenowapi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "servicenowapi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "servicenowapi.dll"]