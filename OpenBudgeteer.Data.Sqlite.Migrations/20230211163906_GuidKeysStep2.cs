#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Data.Sqlite.Migrations;

public partial class GuidKeysStep2 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var generateGuid = @"lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-' || hex(randomblob(6)))";

        migrationBuilder.Sql($@"UPDATE Account SET AccountGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE BankTransaction SET TransactionGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE Bucket SET BucketGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE BucketGroup SET BucketGroupGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE BucketMovement SET BucketMovementGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE BucketRuleSet SET BucketRuleSetGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE BucketVersion SET BucketVersionGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE BudgetedTransaction SET BudgetedTransactionGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE ImportProfile SET ImportProfileGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE MappingRule SET MappingRuleGuidId = {generateGuid}");

        migrationBuilder.Sql($@"UPDATE RecurringBankTransaction SET TransactionGuidId = {generateGuid}");

        migrationBuilder.Sql(@"INSERT INTO BucketGroup (BucketGroupId, Name, Position, BucketGroupGuidId) VALUES (0, 'System', 0, '00000000-0000-0000-0000-000000000001')");

        migrationBuilder.Sql(@"UPDATE Bucket SET BucketGuidId = '00000000-0000-0000-0000-000000000001', BucketGroupGuidId = '00000000-0000-0000-0000-000000000001' WHERE BucketId = 1");

        migrationBuilder.Sql(@"UPDATE Bucket SET BucketGuidId = '00000000-0000-0000-0000-000000000002', BucketGroupGuidId = '00000000-0000-0000-0000-000000000001' WHERE BucketId = 2");
           
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
            type: "TEXT",
            nullable: false,
            collation: "ascii_gener al_ci",
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "RecurringBankTransaction",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "BucketRuleSetId",
            table: "MappingRule",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "MappingRuleId",
                table: "MappingRule",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "AccountId",
            table: "ImportProfile",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "ImportProfileId",
                table: "ImportProfile",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "TransactionId",
            table: "BudgetedTransaction",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
            name: "BucketId",
            table: "BudgetedTransaction",
            type: "TEXT",
            nullable: false,
            collation: "ascii_gen   eral_ci",
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "BudgetedTransactionId",
                table: "BudgetedTransaction",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "BucketId",
            table: "BucketVersion",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "BucketVersionId",
                table: "BucketVersion",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "TargetBucketId",
            table: "BucketRuleSet",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "BucketRuleSetId",
                table: "BucketRuleSet",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "BucketId",
            table: "BucketMovement",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "BucketMovementId",
                table: "BucketMovement",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
                name: "BucketGroupId",
                table: "BucketGroup",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "BucketGroupId",
            table: "Bucket",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "BucketId",
                table: "Bucket",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "AccountId",
            table: "BankTransaction",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "BankTransaction",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "Account",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}