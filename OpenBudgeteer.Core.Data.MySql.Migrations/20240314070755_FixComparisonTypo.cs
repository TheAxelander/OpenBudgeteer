using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class FixComparisonTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ComparisionValue",
                table: "MappingRule",
                newName: "ComparisonValue");

            migrationBuilder.RenameColumn(
                name: "ComparisionType",
                table: "MappingRule",
                newName: "ComparisonType");

            migrationBuilder.RenameColumn(
                name: "ComparisionField",
                table: "MappingRule",
                newName: "ComparisonField");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ComparisonValue",
                table: "MappingRule",
                newName: "ComparisionValue");

            migrationBuilder.RenameColumn(
                name: "ComparisonType",
                table: "MappingRule",
                newName: "ComparisionType");

            migrationBuilder.RenameColumn(
                name: "ComparisonField",
                table: "MappingRule",
                newName: "ComparisionField");
        }
    }
}
