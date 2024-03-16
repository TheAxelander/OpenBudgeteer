#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Data.Sqlite.Migrations;

public partial class ImportCreditColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CreditColumnName",
            table: "ImportProfile",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreditColumnName",
            table: "ImportProfile");
    }
}