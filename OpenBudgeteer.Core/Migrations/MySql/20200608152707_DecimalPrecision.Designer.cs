﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Core.Migrations.MySql
{
    [DbContext(typeof(MySqlDatabaseContext))]
    [Migration("20200608152707_DecimalPrecision")]
    partial class DecimalPrecision
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Account", b =>
                {
                    b.Property<int>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("IsActive")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("AccountId");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BankTransaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<string>("Memo")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Payee")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TransactionDate")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("TransactionId");

                    b.ToTable("BankTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.Bucket", b =>
                {
                    b.Property<int>("BucketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("BucketGroupId")
                        .HasColumnType("int");

                    b.Property<bool>("IsInactive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("IsInactiveFrom")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ValidFrom")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("BucketId");

                    b.ToTable("Bucket");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketGroup", b =>
                {
                    b.Property<int>("BucketGroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.HasKey("BucketGroupId");

                    b.ToTable("BucketGroup");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketMovement", b =>
                {
                    b.Property<int>("BucketMovementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<int>("BucketId")
                        .HasColumnType("int");

                    b.Property<string>("MovementDate")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("BucketMovementId");

                    b.ToTable("BucketMovement");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BucketVersion", b =>
                {
                    b.Property<int>("BucketVersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("BucketId")
                        .HasColumnType("int");

                    b.Property<int>("BucketType")
                        .HasColumnType("int");

                    b.Property<int>("BucketTypeXParam")
                        .HasColumnType("int");

                    b.Property<decimal>("BucketTypeYParam")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<string>("BucketTypeZParam")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("BucketVersionId");

                    b.ToTable("BucketVersion");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.BudgetedTransaction", b =>
                {
                    b.Property<int>("BudgetedTransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65, 2)");

                    b.Property<int>("BucketId")
                        .HasColumnType("int");

                    b.Property<int>("TransactionId")
                        .HasColumnType("int");

                    b.HasKey("BudgetedTransactionId");

                    b.ToTable("BudgetedTransaction");
                });

            modelBuilder.Entity("OpenBudgeteer.Core.Models.ImportProfile", b =>
                {
                    b.Property<int>("ImportProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("AmountColumnName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("HeaderRow")
                        .HasColumnType("int");

                    b.Property<string>("MemoColumnName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("PayeeColumnName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ProfileName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TransactionDateColumnName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("ImportProfileId");

                    b.ToTable("ImportProfile");
                });
#pragma warning restore 612, 618
        }
    }
}
