#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Data.Sqlite.Migrations;

public partial class RecurringBankTransaction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RecurringBankTransaction",
            columns: table => new
            {
                TransactionId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                RecurrenceType = table.Column<int>(type: "INTEGER", nullable: false),
                RecurrenceAmount = table.Column<int>(type: "INTEGER", nullable: false),
                FirstOccurenceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                Payee = table.Column<string>(type: "TEXT", nullable: true),
                Memo = table.Column<string>(type: "TEXT", nullable: true),
                Amount = table.Column<decimal>(type: "decimal(65, 2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecurringBankTransaction", x => x.TransactionId);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RecurringBankTransaction");
    }
}