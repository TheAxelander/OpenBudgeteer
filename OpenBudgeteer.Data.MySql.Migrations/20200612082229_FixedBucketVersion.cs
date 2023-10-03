using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Migrations.MySql;

public partial class FixedBucketVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ValidFrom",
            table: "BucketVersion",
            nullable: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ValidFrom",
            table: "BucketVersion");
    }
}