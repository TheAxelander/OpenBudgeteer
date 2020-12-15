using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Migrations.MySql
{
    public partial class ImportProfileDelimiter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Delimiter",
                table: "ImportProfile",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextQualifier",
                table: "ImportProfile",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Delimiter",
                table: "ImportProfile");

            migrationBuilder.DropColumn(
                name: "TextQualifier",
                table: "ImportProfile");
        }
    }
}
