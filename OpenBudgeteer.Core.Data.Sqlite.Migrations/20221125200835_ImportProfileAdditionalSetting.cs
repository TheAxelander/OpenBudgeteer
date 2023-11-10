#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Data.Sqlite.Migrations;

public partial class ImportProfileAdditionalSetting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "AdditionalSettingAmountCleanup",
            table: "ImportProfile",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "AdditionalSettingAmountCleanupValue",
            table: "ImportProfile",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "AdditionalSettingCreditValue",
            table: "ImportProfile",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "CreditColumnIdentifierColumnName",
            table: "ImportProfile",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CreditColumnIdentifierValue",
            table: "ImportProfile",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AdditionalSettingAmountCleanup",
            table: "ImportProfile");

        migrationBuilder.DropColumn(
            name: "AdditionalSettingAmountCleanupValue",
            table: "ImportProfile");

        migrationBuilder.DropColumn(
            name: "AdditionalSettingCreditValue",
            table: "ImportProfile");

        migrationBuilder.DropColumn(
            name: "CreditColumnIdentifierColumnName",
            table: "ImportProfile");

        migrationBuilder.DropColumn(
            name: "CreditColumnIdentifierValue",
            table: "ImportProfile");
    }
}