using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Data.MySql.Migrations;

public partial class BucketColor : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ColorCode",
            table: "Bucket",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ColorCode",
            table: "Bucket");
    }
}