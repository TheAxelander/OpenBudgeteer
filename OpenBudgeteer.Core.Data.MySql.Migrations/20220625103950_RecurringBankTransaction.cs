#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Data.MySql.Migrations;

public partial class RecurringBankTransaction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                name: "RecurringBankTransaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false),
                    RecurrenceAmount = table.Column<int>(type: "int", nullable: false),
                    FirstOccurenceDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Payee = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Memo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(65,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringBankTransaction", x => x.TransactionId);
                })
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RecurringBankTransaction");
    }
}