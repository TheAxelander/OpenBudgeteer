using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Data.Sqlite.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Account",
            columns: table => new
            {
                AccountId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: true),
                IsActive = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Account", x => x.AccountId);
            });

        migrationBuilder.CreateTable(
            name: "BankTransaction",
            columns: table => new
            {
                TransactionId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                AccountId = table.Column<int>(nullable: false),
                TransactionDate = table.Column<DateTime>(nullable: false),
                Payee = table.Column<string>(nullable: true),
                Memo = table.Column<string>(nullable: true),
                Amount = table.Column<decimal>(type: "decimal(65, 2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BankTransaction", x => x.TransactionId);
            });

        migrationBuilder.CreateTable(
            name: "Bucket",
            columns: table => new
            {
                BucketId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: true),
                BucketGroupId = table.Column<int>(nullable: false),
                ColorCode = table.Column<string>(nullable: true),
                ValidFrom = table.Column<DateTime>(nullable: false),
                IsInactive = table.Column<bool>(nullable: false),
                IsInactiveFrom = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bucket", x => x.BucketId);
            });

        migrationBuilder.CreateTable(
            name: "BucketGroup",
            columns: table => new
            {
                BucketGroupId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: true),
                Position = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BucketGroup", x => x.BucketGroupId);
            });

        migrationBuilder.CreateTable(
            name: "BucketMovement",
            columns: table => new
            {
                BucketMovementId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                BucketId = table.Column<int>(nullable: false),
                Amount = table.Column<decimal>(type: "decimal(65, 2)", nullable: false),
                MovementDate = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BucketMovement", x => x.BucketMovementId);
            });

        migrationBuilder.CreateTable(
            name: "BucketRuleSet",
            columns: table => new
            {
                BucketRuleSetId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Priority = table.Column<int>(nullable: false),
                Name = table.Column<string>(nullable: true),
                TargetBucketId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BucketRuleSet", x => x.BucketRuleSetId);
            });

        migrationBuilder.CreateTable(
            name: "BucketVersion",
            columns: table => new
            {
                BucketVersionId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                BucketId = table.Column<int>(nullable: false),
                Version = table.Column<int>(nullable: false),
                BucketType = table.Column<int>(nullable: false),
                BucketTypeXParam = table.Column<int>(nullable: false),
                BucketTypeYParam = table.Column<decimal>(type: "decimal(65, 2)", nullable: false),
                BucketTypeZParam = table.Column<DateTime>(nullable: false),
                Notes = table.Column<string>(nullable: true),
                ValidFrom = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BucketVersion", x => x.BucketVersionId);
            });

        migrationBuilder.CreateTable(
            name: "BudgetedTransaction",
            columns: table => new
            {
                BudgetedTransactionId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TransactionId = table.Column<int>(nullable: false),
                BucketId = table.Column<int>(nullable: false),
                Amount = table.Column<decimal>(type: "decimal(65, 2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BudgetedTransaction", x => x.BudgetedTransactionId);
            });

        migrationBuilder.CreateTable(
            name: "ImportProfile",
            columns: table => new
            {
                ImportProfileId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ProfileName = table.Column<string>(nullable: true),
                AccountId = table.Column<int>(nullable: false),
                HeaderRow = table.Column<int>(nullable: false),
                Delimiter = table.Column<char>(nullable: false),
                TextQualifier = table.Column<char>(nullable: false),
                DateFormat = table.Column<string>(nullable: true),
                NumberFormat = table.Column<string>(nullable: true),
                TransactionDateColumnName = table.Column<string>(nullable: true),
                PayeeColumnName = table.Column<string>(nullable: true),
                MemoColumnName = table.Column<string>(nullable: true),
                AmountColumnName = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportProfile", x => x.ImportProfileId);
            });

        migrationBuilder.CreateTable(
            name: "MappingRule",
            columns: table => new
            {
                MappingRuleId = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
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
            name: "Account");

        migrationBuilder.DropTable(
            name: "BankTransaction");

        migrationBuilder.DropTable(
            name: "Bucket");

        migrationBuilder.DropTable(
            name: "BucketGroup");

        migrationBuilder.DropTable(
            name: "BucketMovement");

        migrationBuilder.DropTable(
            name: "BucketRuleSet");

        migrationBuilder.DropTable(
            name: "BucketVersion");

        migrationBuilder.DropTable(
            name: "BudgetedTransaction");

        migrationBuilder.DropTable(
            name: "ImportProfile");

        migrationBuilder.DropTable(
            name: "MappingRule");
    }
}