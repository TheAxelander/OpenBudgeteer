#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Data.Postgres.Migrations;

/// <inheritdoc />
public partial class InitialData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "BucketGroup",
            columns: new[] { "BucketGroupId", "Name", "Position" },
            values: new object[] { Guid.Parse("00000000-0000-0000-0000-000000000001"), "System", 0 });
        migrationBuilder.InsertData(
            table: "Bucket",
            columns: new[] { "BucketId", "Name", "BucketGroupId", "ValidFrom", "IsInactive", "IsInactiveFrom" },
            values: new object[] { Guid.Parse("00000000-0000-0000-0000-000000000001"), "Income", Guid.Parse("00000000-0000-0000-0000-000000000001"), new DateTime(1990,1,1), false, DateTime.MaxValue });
        migrationBuilder.InsertData(
            table: "Bucket",
            columns: new[] { "BucketId", "Name", "BucketGroupId", "ValidFrom", "IsInactive", "IsInactiveFrom" },
            values: new object[] { Guid.Parse("00000000-0000-0000-0000-000000000002"), "Transfer", Guid.Parse("00000000-0000-0000-0000-000000000001"), new DateTime(1990,1,1), false, DateTime.MaxValue });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "BucketGroup",
            keyColumn: "BucketGroupId",
            keyValue: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        
        migrationBuilder.DeleteData(
            table: "Bucket",
            keyColumn: "BucketId",
            keyValue: Guid.Parse("00000000-0000-0000-0000-000000000001"));

        migrationBuilder.DeleteData(
            table: "Bucket",
            keyColumn: "BucketId",
            keyValue: Guid.Parse("00000000-0000-0000-0000-000000000002"));
    }
}