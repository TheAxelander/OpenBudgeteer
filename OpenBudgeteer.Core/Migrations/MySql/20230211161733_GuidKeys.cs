using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Migrations.MySql
{
    public partial class GuidKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstOccurenceDate",
                table: "RecurringBankTransaction",
                newName: "FirstOccurrenceDate");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountGuidId",
                table: "RecurringBankTransaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionGuidId",
                table: "RecurringBankTransaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketRuleSetGuidId",
                table: "MappingRule",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MappingRuleGuidId",
                table: "MappingRule",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountGuidId",
                table: "ImportProfile",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ImportProfileGuidId",
                table: "ImportProfile",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketGuidId",
                table: "BudgetedTransaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetedTransactionGuidId",
                table: "BudgetedTransaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionGuidId",
                table: "BudgetedTransaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
            
            migrationBuilder.AddColumn<Guid>(
                name: "BucketGuidId",
                table: "BucketVersion",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketVersionGuidId",
                table: "BucketVersion",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketRuleSetGuidId",
                table: "BucketRuleSet",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetBucketGuidId",
                table: "BucketRuleSet",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketGuidId",
                table: "BucketMovement",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketMovementGuidId",
                table: "BucketMovement",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketGroupGuidId",
                table: "BucketGroup",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketGroupGuidId",
                table: "Bucket",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BucketGuidId",
                table: "Bucket",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountGuidId",
                table: "BankTransaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionGuidId",
                table: "BankTransaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountGuidId",
                table: "Account",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");


            migrationBuilder.Sql(@"UPDATE Account SET AccountGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE BankTransaction SET TransactionGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE Bucket SET BucketGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE BucketGroup SET BucketGroupGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE BucketMovement SET BucketMovementGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE BucketRuleSet SET BucketRuleSetGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE BucketVersion SET BucketVersionGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE BudgetedTransaction SET BudgetedTransactionGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE ImportProfile SET ImportProfileGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE MappingRule SET MappingRuleGuidId = UUID()");

            migrationBuilder.Sql(@"UPDATE RecurringBankTransaction SET TransactionGuidId = UUID()");

            migrationBuilder.InsertData(
                table: "BucketGroup",
                columns: new[] { "BucketGroupId", "Name", "Position", "BucketGroupGuidId" },
                values: new object[] { 0, "System", 0, Guid.Parse("00000000-0000-0000-0000-000000000001") });

            migrationBuilder.UpdateData(
                table: "Bucket",
                column: "BucketGroupGuidId",
                value: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                keyColumn: "BucketGroupId",
                keyValue: "0");

            migrationBuilder.UpdateData(
                table: "Bucket",
                column: "BucketGuidId",
                value: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                keyColumn: "BucketId",
                keyValue: "1");

            migrationBuilder.UpdateData(
                table: "Bucket",
                column: "BucketGuidId",
                value: Guid.Parse("00000000-0000-0000-0000-000000000002"),
                keyColumn: "BucketId",
                keyValue: "2");

            migrationBuilder.Sql(@"UPDATE BankTransaction SET AccountGuidId = (SELECT Account.AccountGuidId FROM Account WHERE Account.AccountId = BankTransaction.AccountId)");

            migrationBuilder.Sql(@"UPDATE Bucket SET BucketGroupGuidId = (SELECT BucketGroup.BucketGroupGuidId FROM BucketGroup WHERE BucketGroup.BucketGroupId = Bucket.BucketGroupId) WHERE BucketGroupId > 0");

            migrationBuilder.Sql(@"UPDATE BucketMovement SET BucketGuidId = (SELECT Bucket.BucketGuidId FROM Bucket WHERE Bucket.BucketId = BucketMovement.BucketId)");

            migrationBuilder.Sql(@"UPDATE BucketRuleSet SET TargetBucketGuidId = (SELECT Bucket.BucketGuidId FROM Bucket WHERE Bucket.BucketId = BucketRuleSet.TargetBucketId)");

            migrationBuilder.Sql(@"UPDATE BucketVersion SET BucketGuidId = (SELECT Bucket.BucketGuidId FROM Bucket WHERE Bucket.BucketId = BucketVersion.BucketId)");

            migrationBuilder.Sql(@"UPDATE BudgetedTransaction SET TransactionGuidId = (SELECT BankTransaction.TransactionGuidId FROM BankTransaction WHERE BankTransaction.TransactionId = BudgetedTransaction.TransactionId)");

            migrationBuilder.Sql(@"UPDATE BudgetedTransaction SET BucketGuidId = (SELECT Bucket.BucketGuidId FROM Bucket WHERE Bucket.BucketId = BudgetedTransaction.BucketId)");

            migrationBuilder.Sql(@"UPDATE ImportProfile SET AccountGuidId = (SELECT Account.AccountGuidId FROM Account WHERE Account.AccountId = ImportProfile.AccountId)");

            migrationBuilder.Sql(@"UPDATE MappingRule SET BucketRuleSetGuidId = (SELECT BucketRuleSet.BucketRuleSetGuidId FROM BucketRuleSet WHERE BucketRuleSet.BucketRuleSetId = MappingRule.BucketRuleSetId)");

            migrationBuilder.Sql(@"UPDATE RecurringBankTransaction SET AccountGuidId = (SELECT Account.AccountGuidId FROM Account WHERE Account.AccountId = RecurringBankTransaction.AccountId)");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "RecurringBankTransaction",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "RecurringBankTransaction",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketRuleSetId",
                table: "MappingRule",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "MappingRuleId",
                table: "MappingRule",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "ImportProfile",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportProfileId",
                table: "ImportProfile",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "BudgetedTransaction",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketId",
                table: "BudgetedTransaction",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "BudgetedTransactionId",
                table: "BudgetedTransaction",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketId",
                table: "BucketVersion",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketVersionId",
                table: "BucketVersion",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "TargetBucketId",
                table: "BucketRuleSet",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketRuleSetId",
                table: "BucketRuleSet",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketId",
                table: "BucketMovement",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketMovementId",
                table: "BucketMovement",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketGroupId",
                table: "BucketGroup",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketGroupId",
                table: "Bucket",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "BucketId",
                table: "Bucket",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "BankTransaction",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "BankTransaction",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "Account",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.Sql(@"UPDATE Account SET AccountId = AccountGuidId");
            migrationBuilder.Sql(@"UPDATE BankTransaction SET TransactionId = TransactionGuidId, AccountId = AccountGuidId");
            migrationBuilder.Sql(@"UPDATE Bucket SET BucketId = BucketGuidId, BucketGroupId = BucketGroupGuidId");
            migrationBuilder.Sql(@"UPDATE BucketGroup SET BucketGroupId = BucketGroupGuidId");
            migrationBuilder.Sql(@"UPDATE BucketMovement SET BucketMovementId = BucketMovementGuidId, BucketId = BucketGuidId");
            migrationBuilder.Sql(@"UPDATE BucketRuleSet SET BucketRuleSetId = BucketRuleSetGuidId, TargetBucketId = TargetBucketGuidId");
            migrationBuilder.Sql(@"UPDATE BucketVersion SET BucketVersionId = BucketVersionGuidId, BucketId = BucketGuidId");
            migrationBuilder.Sql(@"UPDATE BudgetedTransaction SET BudgetedTransactionId = BudgetedTransactionGuidId, TransactionId = TransactionGuidId, BucketId = BucketGuidId");
            migrationBuilder.Sql(@"UPDATE ImportProfile SET ImportProfileId = ImportProfileGuidId, AccountId = AccountGuidId");
            migrationBuilder.Sql(@"UPDATE MappingRule SET MappingRuleId = MappingRuleGuidId, BucketRuleSetId = BucketRuleSetGuidId");
            migrationBuilder.Sql(@"UPDATE RecurringBankTransaction SET TransactionId = TransactionGuidId, AccountId = AccountGuidId");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountGuidId",
                table: "RecurringBankTransaction");

            migrationBuilder.DropColumn(
                name: "TransactionGuidId",
                table: "RecurringBankTransaction");

            migrationBuilder.DropColumn(
                name: "BucketRuleSetGuidId",
                table: "MappingRule");

            migrationBuilder.DropColumn(
                name: "MappingRuleGuidId",
                table: "MappingRule");

            migrationBuilder.DropColumn(
                name: "AccountGuidId",
                table: "ImportProfile");

            migrationBuilder.DropColumn(
                name: "ImportProfileGuidId",
                table: "ImportProfile");

            migrationBuilder.DropColumn(
                name: "BucketGuidId",
                table: "BudgetedTransaction");

            migrationBuilder.DropColumn(
                name: "BudgetedTransactionGuidId",
                table: "BudgetedTransaction");

            migrationBuilder.DropColumn(
                name: "TransactionGuidId",
                table: "BudgetedTransaction");

            migrationBuilder.DropColumn(
                name: "BucketGuidId",
                table: "BucketVersion");

            migrationBuilder.DropColumn(
                name: "BucketVersionGuidId",
                table: "BucketVersion");

            migrationBuilder.DropColumn(
                name: "BucketRuleSetGuidId",
                table: "BucketRuleSet");

            migrationBuilder.DropColumn(
                name: "TargetBucketGuidId",
                table: "BucketRuleSet");

            migrationBuilder.DropColumn(
                name: "BucketGuidId",
                table: "BucketMovement");

            migrationBuilder.DropColumn(
                name: "BucketMovementGuidId",
                table: "BucketMovement");

            migrationBuilder.DropColumn(
                name: "BucketGroupGuidId",
                table: "BucketGroup");

            migrationBuilder.DropColumn(
                name: "BucketGroupGuidId",
                table: "Bucket");

            migrationBuilder.DropColumn(
                name: "BucketGuidId",
                table: "Bucket");

            migrationBuilder.DropColumn(
                name: "AccountGuidId",
                table: "BankTransaction");

            migrationBuilder.DropColumn(
                name: "TransactionGuidId",
                table: "BankTransaction");

            migrationBuilder.DropColumn(
                name: "AccountGuidId",
                table: "Account");

            migrationBuilder.RenameColumn(
                name: "FirstOccurrenceDate",
                table: "RecurringBankTransaction",
                newName: "FirstOccurenceDate");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "RecurringBankTransaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "TransactionId",
                table: "RecurringBankTransaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketRuleSetId",
                table: "MappingRule",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "MappingRuleId",
                table: "MappingRule",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "ImportProfile",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "ImportProfileId",
                table: "ImportProfile",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "TransactionId",
                table: "BudgetedTransaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketId",
                table: "BudgetedTransaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BudgetedTransactionId",
                table: "BudgetedTransaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketId",
                table: "BucketVersion",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketVersionId",
                table: "BucketVersion",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "TargetBucketId",
                table: "BucketRuleSet",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketRuleSetId",
                table: "BucketRuleSet",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketId",
                table: "BucketMovement",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketMovementId",
                table: "BucketMovement",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketGroupId",
                table: "BucketGroup",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketGroupId",
                table: "Bucket",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BucketId",
                table: "Bucket",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "BankTransaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "TransactionId",
                table: "BankTransaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "Account",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
