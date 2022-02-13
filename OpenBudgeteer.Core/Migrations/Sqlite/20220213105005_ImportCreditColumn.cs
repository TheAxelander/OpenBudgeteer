using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Migrations.Sqlite
{
    public partial class ImportCreditColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreditColumnName",
                table: "ImportProfile",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPositiveCreditColumnValues",
                table: "ImportProfile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditColumnName",
                table: "ImportProfile");

            migrationBuilder.DropColumn(
                name: "HasPositiveCreditColumnValues",
                table: "ImportProfile");
        }
    }
}
