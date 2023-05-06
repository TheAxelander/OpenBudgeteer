﻿// <auto-generated />

#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OpenBudgeteer.Core.Common.Database;

namespace OpenBudgeteer.Data.Sqlite.Migrations
{
    [DbContext(typeof(SqliteDatabaseContext))]
    [Migration("20220213195523_ImportCreditColumn")]
    partial class ImportCreditColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.0");

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Account", b =>
                {
                    b.Property<int>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("AccountId");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BankTransaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<string>("Memo")
                        .HasColumnType("TEXT");

                    b.Property<string>("Payee")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("TEXT");

                    b.HasKey("TransactionId");

                    b.ToTable("BankTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Bucket", b =>
                {
                    b.Property<int>("BucketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BucketGroupId")
                        .HasColumnType("INTEGER");

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
                    b.Property<int>("BucketGroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("BucketGroupId");

                    b.ToTable("BucketGroup");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketMovement", b =>
                {
                    b.Property<int>("BucketMovementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<int>("BucketId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("MovementDate")
                        .HasColumnType("TEXT");

                    b.HasKey("BucketMovementId");

                    b.ToTable("BucketMovement");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketRuleSet", b =>
                {
                    b.Property<int>("BucketRuleSetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Priority")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TargetBucketId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BucketRuleSetId");

                    b.ToTable("BucketRuleSet");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketVersion", b =>
                {
                    b.Property<int>("BucketVersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BucketId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BucketType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BucketTypeXParam")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("BucketTypeYParam")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<DateTime>("BucketTypeZParam")
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
                    b.Property<int>("BudgetedTransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<int>("BucketId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TransactionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BudgetedTransactionId");

                    b.ToTable("BudgetedTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.ImportProfile", b =>
                {
                    b.Property<int>("ImportProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AmountColumnName")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreditColumnName")
                        .HasColumnType("TEXT");

                    b.Property<string>("DateFormat")
                        .HasColumnType("TEXT");

                    b.Property<char>("Delimiter")
                        .HasColumnType("TEXT");

                    b.Property<int>("HeaderRow")
                        .HasColumnType("INTEGER");

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
                    b.Property<int>("MappingRuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BucketRuleSetId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ComparisionField")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ComparisionType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ComparisionValue")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("MappingRuleId");

                    b.ToTable("MappingRule");
                });
#pragma warning restore 612, 618
        }
    }
}
