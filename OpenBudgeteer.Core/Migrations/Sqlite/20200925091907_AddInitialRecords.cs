using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Migrations.Sqlite
{
    public partial class AddInitialRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Bucket",
                columns: new[] { "BucketId", "Name", "BucketGroupId", "ValidFrom", "IsInactive", "IsInactiveFrom" },
                values: new object[] { 1, "Income", 0, "1990-01-01", false, $"{DateTime.MaxValue:yyyy-MM-dd}" });
            migrationBuilder.InsertData(
                table: "Bucket",
                columns: new[] { "BucketId", "Name", "BucketGroupId", "ValidFrom", "IsInactive", "IsInactiveFrom" },
                values: new object[] { 2, "Transfer", 0, "1990-01-01", false, $"{DateTime.MaxValue:yyyy-MM-dd}" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bucket",
                keyColumn: "BucketId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Bucket",
                keyColumn: "BucketId",
                keyValue: 2);
        }
    }
}
