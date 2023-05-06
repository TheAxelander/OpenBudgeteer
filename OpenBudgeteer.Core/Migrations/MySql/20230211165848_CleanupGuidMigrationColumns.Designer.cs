﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenBudgeteer.Data;

#nullable disable

namespace OpenBudgeteer.Core.Migrations.MySql
{
    [DbContext(typeof(MySqlDatabaseContext))]
    [Migration("20230211165848_CleanupGuidMigrationColumns")]
    partial class CleanupGuidMigrationColumns
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4");

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Account", b =>
                {
                    b.Property<Guid>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<int>("IsActive")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("AccountId");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BankTransaction", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65,2)");

                    b.Property<string>("Memo")
                        .HasColumnType("longtext");

                    b.Property<string>("Payee")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime(6)");

                    b.HasKey("TransactionId");

                    b.ToTable("BankTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Bucket", b =>
                {
                    b.Property<Guid>("BucketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("BucketGroupId")
                        .HasColumnType("char(36)");

                    b.Property<string>("ColorCode")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsInactive")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("IsInactiveFrom")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("datetime(6)");

                    b.HasKey("BucketId");

                    b.ToTable("Bucket");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketGroup", b =>
                {
                    b.Property<Guid>("BucketGroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.HasKey("BucketGroupId");

                    b.ToTable("BucketGroup");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketMovement", b =>
                {
                    b.Property<Guid>("BucketMovementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65,2)");

                    b.Property<Guid>("BucketId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("MovementDate")
                        .HasColumnType("datetime(6)");

                    b.HasKey("BucketMovementId");

                    b.ToTable("BucketMovement");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketRuleSet", b =>
                {
                    b.Property<Guid>("BucketRuleSetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<Guid>("TargetBucketId")
                        .HasColumnType("char(36)");

                    b.HasKey("BucketRuleSetId");

                    b.ToTable("BucketRuleSet");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketVersion", b =>
                {
                    b.Property<Guid>("BucketVersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("BucketId")
                        .HasColumnType("char(36)");

                    b.Property<int>("BucketType")
                        .HasColumnType("int");

                    b.Property<int>("BucketTypeXParam")
                        .HasColumnType("int");

                    b.Property<decimal>("BucketTypeYParam")
                        .HasColumnType("decimal(65,2)");

                    b.Property<DateTime>("BucketTypeZParam")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Notes")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("BucketVersionId");

                    b.ToTable("BucketVersion");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BudgetedTransaction", b =>
                {
                    b.Property<Guid>("BudgetedTransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65,2)");

                    b.Property<Guid>("BucketId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("char(36)");

                    b.HasKey("BudgetedTransactionId");

                    b.ToTable("BudgetedTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.ImportProfile", b =>
                {
                    b.Property<Guid>("ImportProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<bool>("AdditionalSettingAmountCleanup")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("AdditionalSettingAmountCleanupValue")
                        .HasColumnType("longtext");

                    b.Property<int>("AdditionalSettingCreditValue")
                        .HasColumnType("int");

                    b.Property<string>("AmountColumnName")
                        .HasColumnType("longtext");

                    b.Property<string>("CreditColumnIdentifierColumnName")
                        .HasColumnType("longtext");

                    b.Property<string>("CreditColumnIdentifierValue")
                        .HasColumnType("longtext");

                    b.Property<string>("CreditColumnName")
                        .HasColumnType("longtext");

                    b.Property<string>("DateFormat")
                        .HasColumnType("longtext");

                    b.Property<string>("Delimiter")
                        .IsRequired()
                        .HasColumnType("varchar(1)");

                    b.Property<int>("HeaderRow")
                        .HasColumnType("int");

                    b.Property<string>("MemoColumnName")
                        .HasColumnType("longtext");

                    b.Property<string>("NumberFormat")
                        .HasColumnType("longtext");

                    b.Property<string>("PayeeColumnName")
                        .HasColumnType("longtext");

                    b.Property<string>("ProfileName")
                        .HasColumnType("longtext");

                    b.Property<string>("TextQualifier")
                        .IsRequired()
                        .HasColumnType("varchar(1)");

                    b.Property<string>("TransactionDateColumnName")
                        .HasColumnType("longtext");

                    b.HasKey("ImportProfileId");

                    b.ToTable("ImportProfile");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.MappingRule", b =>
                {
                    b.Property<Guid>("MappingRuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("BucketRuleSetId")
                        .HasColumnType("char(36)");

                    b.Property<int>("ComparisionField")
                        .HasColumnType("int");

                    b.Property<int>("ComparisionType")
                        .HasColumnType("int");

                    b.Property<string>("ComparisionValue")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("MappingRuleId");

                    b.ToTable("MappingRule");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.RecurringBankTransaction", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65,2)");

                    b.Property<DateTime>("FirstOccurrenceDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Memo")
                        .HasColumnType("longtext");

                    b.Property<string>("Payee")
                        .HasColumnType("longtext");

                    b.Property<int>("RecurrenceAmount")
                        .HasColumnType("int");

                    b.Property<int>("RecurrenceType")
                        .HasColumnType("int");

                    b.HasKey("TransactionId");

                    b.ToTable("RecurringBankTransaction");
                });
#pragma warning restore 612, 618
        }
    }
}
