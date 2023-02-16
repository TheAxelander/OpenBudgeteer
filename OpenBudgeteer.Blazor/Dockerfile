#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OpenBudgeteer.Blazor/OpenBudgeteer.Blazor.csproj", "OpenBudgeteer.Blazor/"]
COPY ["OpenBudgeteer.Core/OpenBudgeteer.Core.csproj", "OpenBudgeteer.Core/"]
RUN dotnet restore "OpenBudgeteer.Blazor/OpenBudgeteer.Blazor.csproj"
COPY . .
WORKDIR "/src/OpenBudgeteer.Blazor"
RUN dotnet build "OpenBudgeteer.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenBudgeteer.Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenBudgeteer.dll"]