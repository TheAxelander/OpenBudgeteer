using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemBucketVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BucketVersion",
                columns: new[] 
                { 
                    "BucketVersionId", "BucketId", 
                    "Version", "BucketType", "BucketTypeXParam", "BucketTypeYParam", 
                    "BucketTypeZParam", "Notes", "ValidFrom" 
                },
                values: new object[]
                {
                    Guid.Parse("00000000-0000-0000-0000-000000000001"), Guid.Parse("00000000-0000-0000-0000-000000000001"), 
                    1, 1, 0, 0,
                    DateTime.MinValue, null, DateTime.MinValue
                });
            migrationBuilder.InsertData(
                table: "BucketVersion",
                columns: new[] 
                { 
                    "BucketVersionId", "BucketId", 
                    "Version", "BucketType", "BucketTypeXParam", "BucketTypeYParam", 
                    "BucketTypeZParam", "Notes", "ValidFrom" 
                },
                values: new object[]
                {
                    Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Parse("00000000-0000-0000-0000-000000000002"), 
                    1, 1, 0, 0,
                    DateTime.MinValue, null, DateTime.MinValue
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BucketVersion",
                keyColumn: "BucketId",
                keyValue: Guid.Parse("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "BucketVersion",
                keyColumn: "BucketId",
                keyValue: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        }
    }
}
