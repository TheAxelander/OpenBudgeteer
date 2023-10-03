using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBudgeteer.Core.Migrations.MySql;

public partial class CleanupGuidMigrationColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
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
    }
}