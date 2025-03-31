using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class BucketIsHiddenFromSummaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHiddenFromSummaries",
                table: "Bucket",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.UpdateData(
                table: "Bucket",
                keyColumn: "BucketId",
                keyValue: Guid.Parse("00000000-0000-0000-0000-000000000002"),
                column: "IsHiddenFromSummaries",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHiddenFromSummaries",
                table: "Bucket");
        }
    }
}
