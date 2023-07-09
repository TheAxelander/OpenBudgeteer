#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 80
EXPOSE 443
RUN apk add --no-cache icu-libs icu-data-full

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 
#WORKDIR /src
#COPY . .
#RUN dotnet restore -r linux-musl-x64
#WORKDIR "/src/OpenBudgeteer.Blazor"
#RUN dotnet publish "OpenBudgeteer.Blazor.csproj" -r linux-musl-x64 --no-self-contained -c Release -o /app/publish

WORKDIR /src
COPY ["OpenBudgeteer.Blazor/OpenBudgeteer.Blazor.csproj", "OpenBudgeteer.Blazor/"]
COPY ["OpenBudgeteer.Core/OpenBudgeteer.Core.csproj", "OpenBudgeteer.Core/"]
COPY ["OpenBudgeteer.Contracts/OpenBudgeteer.Contracts.csproj", "OpenBudgeteer.Contracts/"]
COPY ["OpenBudgeteer.Data/OpenBudgeteer.Data.csproj", "OpenBudgeteer.Data/"]
COPY ["OpenBudgeteer.Data.MySql.Migrations/OpenBudgeteer.Data.MySql.Migrations.csproj", "OpenBudgeteer.Data.MySql.Migrations/"]
COPY ["OpenBudgeteer.Data.Postgres.Migrations/OpenBudgeteer.Data.Postgres.Migrations.csproj", "OpenBudgeteer.Data.Postgres.Migrations/"]
COPY ["OpenBudgeteer.Data.Sqlite.Migrations/OpenBudgeteer.Data.Sqlite.Migrations.csproj", "OpenBudgeteer.Data.Sqlite.Migrations/"]
RUN dotnet restore "OpenBudgeteer.Blazor/OpenBudgeteer.Blazor.csproj"
COPY . .
WORKDIR "/src/OpenBudgeteer.Blazor"
RUN dotnet build "OpenBudgeteer.Blazor.csproj" -c Release -o /app/build

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenBudgeteer.dll"]