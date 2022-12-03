using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Migrations.MySql
{
    public partial class ImportProfileAdditionalSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdditionalSettingAmountCleanup",
                table: "ImportProfile",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalSettingAmountCleanupValue",
                table: "ImportProfile",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "AdditionalSettingCreditValue",
                table: "ImportProfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreditColumnIdentifierColumnName",
                table: "ImportProfile",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreditColumnIdentifierValue",
                table: "ImportProfile",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
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
}
