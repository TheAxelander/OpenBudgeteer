using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Migrations.MySql;

public partial class ImportProfileDateNumberFormat : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DateFormat",
            table: "ImportProfile",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "NumberFormat",
            table: "ImportProfile",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DateFormat",
            table: "ImportProfile");

        migrationBuilder.DropColumn(
            name: "NumberFormat",
            table: "ImportProfile");
    }
}