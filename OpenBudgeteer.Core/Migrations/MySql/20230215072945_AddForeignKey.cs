using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Migrations.MySql
{
    public partial class AddForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RecurringBankTransaction_AccountId",
                table: "RecurringBankTransaction",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingRule_BucketRuleSetId",
                table: "MappingRule",
                column: "BucketRuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportProfile_AccountId",
                table: "ImportProfile",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetedTransaction_BucketId",
                table: "BudgetedTransaction",
                column: "BucketId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetedTransaction_TransactionId",
                table: "BudgetedTransaction",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BucketVersion_BucketId",
                table: "BucketVersion",
                column: "BucketId");

            migrationBuilder.CreateIndex(
                name: "IX_BucketRuleSet_TargetBucketId",
                table: "BucketRuleSet",
                column: "TargetBucketId");

            migrationBuilder.CreateIndex(
                name: "IX_BucketMovement_BucketId",
                table: "BucketMovement",
                column: "BucketId");

            migrationBuilder.CreateIndex(
                name: "IX_Bucket_BucketGroupId",
                table: "Bucket",
                column: "BucketGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransaction_AccountId",
                table: "BankTransaction",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransaction_Account_AccountId",
                table: "BankTransaction",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bucket_BucketGroup_BucketGroupId",
                table: "Bucket",
                column: "BucketGroupId",
                principalTable: "BucketGroup",
                principalColumn: "BucketGroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BucketMovement_Bucket_BucketId",
                table: "BucketMovement",
                column: "BucketId",
                principalTable: "Bucket",
                principalColumn: "BucketId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BucketRuleSet_Bucket_TargetBucketId",
                table: "BucketRuleSet",
                column: "TargetBucketId",
                principalTable: "Bucket",
                principalColumn: "BucketId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BucketVersion_Bucket_BucketId",
                table: "BucketVersion",
                column: "BucketId",
                principalTable: "Bucket",
                principalColumn: "BucketId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetedTransaction_BankTransaction_TransactionId",
                table: "BudgetedTransaction",
                column: "TransactionId",
                principalTable: "BankTransaction",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetedTransaction_Bucket_BucketId",
                table: "BudgetedTransaction",
                column: "BucketId",
                principalTable: "Bucket",
                principalColumn: "BucketId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportProfile_Account_AccountId",
                table: "ImportProfile",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MappingRule_BucketRuleSet_BucketRuleSetId",
                table: "MappingRule",
                column: "BucketRuleSetId",
                principalTable: "BucketRuleSet",
                principalColumn: "BucketRuleSetId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringBankTransaction_Account_AccountId",
                table: "RecurringBankTransaction",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_Account_AccountId",
                table: "BankTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Bucket_BucketGroup_BucketGroupId",
                table: "Bucket");

            migrationBuilder.DropForeignKey(
                name: "FK_BucketMovement_Bucket_BucketId",
                table: "BucketMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_BucketRuleSet_Bucket_TargetBucketId",
                table: "BucketRuleSet");

            migrationBuilder.DropForeignKey(
                name: "FK_BucketVersion_Bucket_BucketId",
                table: "BucketVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetedTransaction_BankTransaction_TransactionId",
                table: "BudgetedTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetedTransaction_Bucket_BucketId",
                table: "BudgetedTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportProfile_Account_AccountId",
                table: "ImportProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_MappingRule_BucketRuleSet_BucketRuleSetId",
                table: "MappingRule");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringBankTransaction_Account_AccountId",
                table: "RecurringBankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_RecurringBankTransaction_AccountId",
                table: "RecurringBankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_MappingRule_BucketRuleSetId",
                table: "MappingRule");

            migrationBuilder.DropIndex(
                name: "IX_ImportProfile_AccountId",
                table: "ImportProfile");

            migrationBuilder.DropIndex(
                name: "IX_BudgetedTransaction_BucketId",
                table: "BudgetedTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BudgetedTransaction_TransactionId",
                table: "BudgetedTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BucketVersion_BucketId",
                table: "BucketVersion");

            migrationBuilder.DropIndex(
                name: "IX_BucketRuleSet_TargetBucketId",
                table: "BucketRuleSet");

            migrationBuilder.DropIndex(
                name: "IX_BucketMovement_BucketId",
                table: "BucketMovement");

            migrationBuilder.DropIndex(
                name: "IX_Bucket_BucketGroupId",
                table: "Bucket");

            migrationBuilder.DropIndex(
                name: "IX_BankTransaction_AccountId",
                table: "BankTransaction");
        }
    }
}
