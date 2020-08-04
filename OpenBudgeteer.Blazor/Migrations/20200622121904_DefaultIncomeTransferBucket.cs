using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Blazor.Migrations
{
    public partial class DefaultIncomeTransferBucket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Bucket",
                columns: new[] { "BucketId", "Name", "BucketGroupId", "ValidFrom", "IsInactive" },
                values: new object[] { 1, "Income", 0, "1990-01-01", false });
            migrationBuilder.InsertData(
                table: "Bucket",
                columns: new[] { "BucketId", "Name", "BucketGroupId", "ValidFrom", "IsInactive" },
                values: new object[] { 2, "Transfer", 0, "1990-01-01", false });
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
