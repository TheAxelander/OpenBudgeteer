FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /api
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs icu-data-full

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 
WORKDIR /src
COPY ["OpenBudgeteer.API/OpenBudgeteer.API.csproj", "OpenBudgeteer.API/"]
RUN dotnet restore "OpenBudgeteer.API/OpenBudgeteer.API.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/OpenBudgeteer.API"
RUN dotnet publish "OpenBudgeteer.API.csproj" --no-self-contained -c Release -a $TARGETARCH -o /api/publish

FROM base AS final
WORKDIR /api
COPY --from=build /api/publish .
ENTRYPOINT ["dotnet", "OpenBudgeteer.API.dll"]
