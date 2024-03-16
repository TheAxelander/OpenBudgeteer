using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Core.Data.MySql.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Account",
            columns: table => new
            {
                AccountId = table.Column<int>(nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                AccountId = table.Column<int>(nullable: false),
                TransactionDate = table.Column<string>(nullable: true),
                Payee = table.Column<string>(nullable: true),
                Memo = table.Column<string>(nullable: true),
                Amount = table.Column<decimal>(nullable: false)
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
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(nullable: true),
                BucketGroupId = table.Column<int>(nullable: false),
                ValidFrom = table.Column<string>(nullable: false),
                IsInactive = table.Column<bool>(nullable: false),
                IsInactiveFrom = table.Column<string>(nullable: true)
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
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                BucketId = table.Column<int>(nullable: false),
                Amount = table.Column<decimal>(nullable: false),
                MovementDate = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BucketMovement", x => x.BucketMovementId);
            });

        migrationBuilder.CreateTable(
            name: "BucketVersion",
            columns: table => new
            {
                BucketVersionId = table.Column<int>(nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                BucketId = table.Column<int>(nullable: false),
                Version = table.Column<int>(nullable: false),
                BucketType = table.Column<int>(nullable: false),
                BucketTypeXParam = table.Column<int>(nullable: false),
                BucketTypeYParam = table.Column<decimal>(nullable: false),
                BucketTypeZParam = table.Column<string>(nullable: true)
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
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                TransactionId = table.Column<int>(nullable: false),
                BucketId = table.Column<int>(nullable: false),
                Amount = table.Column<decimal>(nullable: false)
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
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ProfileName = table.Column<string>(nullable: true),
                AccountId = table.Column<int>(nullable: false),
                HeaderRow = table.Column<int>(nullable: false),
                TransactionDateColumnName = table.Column<string>(nullable: true),
                PayeeColumnName = table.Column<string>(nullable: true),
                MemoColumnName = table.Column<string>(nullable: true),
                AmountColumnName = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportProfile", x => x.ImportProfileId);
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
            name: "BucketVersion");

        migrationBuilder.DropTable(
            name: "BudgetedTransaction");

        migrationBuilder.DropTable(
            name: "ImportProfile");
    }
}