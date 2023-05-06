#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Data.Sqlite.Migrations;

public partial class GuidKeysStep3 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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

    }
}