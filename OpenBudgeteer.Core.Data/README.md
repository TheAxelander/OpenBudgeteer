# OpenBudgeteer.Core.Data

## Create new Migration

- Update `appsettings.json` to connect to a running Db instance
- Run below command

For MySql:
```shell
dotnet ef migrations add __MigrationName__ --project ../OpenBudgeteer.Core.Data.MySql.Migrations/OpenBudgeteer.Core.Data.MySql.Migrations.csproj
```

For Postgres:
```shell
dotnet ef migrations add __MigrationName__ --project ../OpenBudgeteer.Core.Data.Postgres.Migrations/OpenBudgeteer.Core.Data.Postgres.Migrations.csproj
```