using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Migrations.MySql
{
    public partial class AutomaticBucketAssignment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BucketRuleSet",
                columns: table => new
                {
                    BucketRuleSetId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Priority = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    TargetBucketId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BucketRuleSet", x => x.BucketRuleSetId);
                });

            migrationBuilder.CreateTable(
                name: "MappingRule",
                columns: table => new
                {
                    MappingRuleId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BucketRuleSetId = table.Column<int>(nullable: false),
                    ComparisionField = table.Column<int>(nullable: false),
                    ComparisionType = table.Column<int>(nullable: false),
                    ComparisionValue = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingRule", x => x.MappingRuleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BucketRuleSet");

            migrationBuilder.DropTable(
                name: "MappingRule");
        }
    }
}
