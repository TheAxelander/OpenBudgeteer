FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 
WORKDIR /src
COPY . .
RUN dotnet restore -a $TARGETARCH
WORKDIR "/src/OpenBudgeteer.Blazor"
RUN dotnet publish "OpenBudgeteer.Blazor.csproj" --no-self-contained -c Release -a $TARGETARCH -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenBudgeteer.dll"]