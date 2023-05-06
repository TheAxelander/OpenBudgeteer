﻿// <auto-generated />

#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBudgeteer.Data.Sqlite.Migrations
{
    [DbContext(typeof(SqliteDatabaseContext))]
    [Migration("20230211163815_GuidKeysStep1")]
    partial class GuidKeysStep1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.2");

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Account", b =>
                {
                    b.Property<Guid>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountGuidId")
                        .HasColumnType("TEXT");

                    b.Property<int>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("AccountId");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BankTransaction", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<string>("Memo")
                        .HasColumnType("TEXT");

                    b.Property<string>("Payee")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TransactionGuidId")
                        .HasColumnType("TEXT");

                    b.HasKey("TransactionId");

                    b.ToTable("BankTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Bucket", b =>
                {
                    b.Property<Guid>("BucketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketGroupGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketGroupId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketGuidId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ColorCode")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsInactive")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("IsInactiveFrom")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("TEXT");

                    b.HasKey("BucketId");

                    b.ToTable("Bucket");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketGroup", b =>
                {
                    b.Property<Guid>("BucketGroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketGroupGuidId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("BucketGroupId");

                    b.ToTable("BucketGroup");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketMovement", b =>
                {
                    b.Property<Guid>("BucketMovementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<Guid>("BucketGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketMovementGuidId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("MovementDate")
                        .HasColumnType("TEXT");

                    b.HasKey("BucketMovementId");

                    b.ToTable("BucketMovement");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketRuleSet", b =>
                {
                    b.Property<Guid>("BucketRuleSetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketRuleSetGuidId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Priority")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("TargetBucketGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TargetBucketId")
                        .HasColumnType("TEXT");

                    b.HasKey("BucketRuleSetId");

                    b.ToTable("BucketRuleSet");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketVersion", b =>
                {
                    b.Property<Guid>("BucketVersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketId")
                        .HasColumnType("TEXT");

                    b.Property<int>("BucketType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BucketTypeXParam")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("BucketTypeYParam")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<DateTime>("BucketTypeZParam")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketVersionGuidId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("TEXT");

                    b.Property<int>("Version")
                        .HasColumnType("INTEGER");

                    b.HasKey("BucketVersionId");

                    b.ToTable("BucketVersion");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BudgetedTransaction", b =>
                {
                    b.Property<Guid>("BudgetedTransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<Guid>("BucketGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BudgetedTransactionGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TransactionGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("TEXT");

                    b.HasKey("BudgetedTransactionId");

                    b.ToTable("BudgetedTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.ImportProfile", b =>
                {
                    b.Property<Guid>("ImportProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AdditionalSettingAmountCleanup")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AdditionalSettingAmountCleanupValue")
                        .HasColumnType("TEXT");

                    b.Property<int>("AdditionalSettingCreditValue")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AmountColumnName")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreditColumnIdentifierColumnName")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreditColumnIdentifierValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreditColumnName")
                        .HasColumnType("TEXT");

                    b.Property<string>("DateFormat")
                        .HasColumnType("TEXT");

                    b.Property<char>("Delimiter")
                        .HasColumnType("TEXT");

                    b.Property<int>("HeaderRow")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ImportProfileGuidId")
                        .HasColumnType("TEXT");

                    b.Property<string>("MemoColumnName")
                        .HasColumnType("TEXT");

                    b.Property<string>("NumberFormat")
                        .HasColumnType("TEXT");

                    b.Property<string>("PayeeColumnName")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProfileName")
                        .HasColumnType("TEXT");

                    b.Property<char>("TextQualifier")
                        .HasColumnType("TEXT");

                    b.Property<string>("TransactionDateColumnName")
                        .HasColumnType("TEXT");

                    b.HasKey("ImportProfileId");

                    b.ToTable("ImportProfile");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.MappingRule", b =>
                {
                    b.Property<Guid>("MappingRuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketRuleSetGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BucketRuleSetId")
                        .HasColumnType("TEXT");

                    b.Property<int>("ComparisionField")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ComparisionType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ComparisionValue")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("MappingRuleGuidId")
                        .HasColumnType("TEXT");

                    b.HasKey("MappingRuleId");

                    b.ToTable("MappingRule");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.RecurringBankTransaction", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountGuidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<DateTime>("FirstOccurrenceDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Memo")
                        .HasColumnType("TEXT");

                    b.Property<string>("Payee")
                        .HasColumnType("TEXT");

                    b.Property<int>("RecurrenceAmount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RecurrenceType")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("TransactionGuidId")
                        .HasColumnType("TEXT");

                    b.HasKey("TransactionId");

                    b.ToTable("RecurringBankTransaction");
                });
#pragma warning restore 612, 618
        }
    }
}
