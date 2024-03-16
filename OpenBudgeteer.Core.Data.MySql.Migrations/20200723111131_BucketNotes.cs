using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Data.MySql.Migrations;

public partial class BucketNotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Notes",
            table: "BucketVersion",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Notes",
            table: "BucketVersion");
    }
}