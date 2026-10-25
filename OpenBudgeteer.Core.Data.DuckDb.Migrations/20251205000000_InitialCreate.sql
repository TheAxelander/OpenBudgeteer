-- Initial schema creation for DuckDB

-- Foreign Keys have been disabled for now due to "Over-Eager Constraint Checking in Foreign Keys"
-- which leads to failing Database Unit Tests with "Duplicate Primary Key"
-- see: https://duckdb.org/docs/stable/sql/indexes#over-eager-constraint-checking-in-foreign-keys

-- Migrations tracking table (similar to EF Core's __EFMigrationsHistory)
CREATE TABLE IF NOT EXISTS __DuckDbMigrations (
    MigrationId VARCHAR PRIMARY KEY,
    AppliedOn TIMESTAMP NOT NULL
);

-- Account table
CREATE TABLE IF NOT EXISTS Account (
    AccountId UUID PRIMARY KEY,
    Name VARCHAR,
    IsActive INTEGER NOT NULL
);

-- BucketGroup table
CREATE TABLE IF NOT EXISTS BucketGroup (
    BucketGroupId UUID PRIMARY KEY,
    Name VARCHAR,
    Position INTEGER NOT NULL
);

-- BankTransaction table
CREATE TABLE IF NOT EXISTS BankTransaction (
    TransactionId UUID PRIMARY KEY,
    AccountId UUID NOT NULL,
    TransactionDate DATE NOT NULL,
    Payee VARCHAR,
    Memo VARCHAR,
    Amount DECIMAL(38, 2) NOT NULL,
    --FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
);

--CREATE INDEX IF NOT EXISTS IX_BankTransaction_AccountId ON BankTransaction(AccountId);

-- ImportProfile table
CREATE TABLE IF NOT EXISTS ImportProfile (
    ImportProfileId UUID PRIMARY KEY,
    ProfileName VARCHAR,
    AccountId UUID NOT NULL,
    HeaderRow INTEGER NOT NULL,
    Delimiter VARCHAR(1) NOT NULL,
    TextQualifier VARCHAR(1) NOT NULL,
    DateFormat VARCHAR,
    NumberFormat VARCHAR,
    TransactionDateColumnName VARCHAR,
    PayeeColumnName VARCHAR,
    MemoColumnName VARCHAR,
    AmountColumnName VARCHAR,
    AdditionalSettingCreditValue INTEGER NOT NULL,
    CreditColumnName VARCHAR,
    CreditColumnIdentifierColumnName VARCHAR,
    CreditColumnIdentifierValue VARCHAR,
    AdditionalSettingAmountCleanup BOOLEAN NOT NULL,
    AdditionalSettingAmountCleanupValue VARCHAR,
    --FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
);

--CREATE INDEX IF NOT EXISTS IX_ImportProfile_AccountId ON ImportProfile(AccountId);

-- RecurringBankTransaction table
CREATE TABLE IF NOT EXISTS RecurringBankTransaction (
    TransactionId UUID PRIMARY KEY,
    AccountId UUID NOT NULL,
    RecurrenceType INTEGER NOT NULL,
    RecurrenceAmount INTEGER NOT NULL,
    FirstOccurrenceDate DATE NOT NULL,
    Payee VARCHAR,
    Memo VARCHAR,
    Amount DECIMAL(38, 2) NOT NULL,
    --FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
);

--CREATE INDEX IF NOT EXISTS IX_RecurringBankTransaction_AccountId ON RecurringBankTransaction(AccountId);

-- Bucket table
CREATE TABLE IF NOT EXISTS Bucket (
    BucketId UUID PRIMARY KEY,
    Name VARCHAR,
    BucketGroupId UUID NOT NULL,
    ColorCode VARCHAR,
    TextColorCode VARCHAR,
    ValidFrom DATE NOT NULL,
    IsInactive BOOLEAN NOT NULL,
    IsInactiveFrom DATE NOT NULL,
    IsHiddenFromSummaries BOOLEAN NOT NULL DEFAULT false,
    --FOREIGN KEY (BucketGroupId) REFERENCES BucketGroup(BucketGroupId)
);

--CREATE INDEX IF NOT EXISTS IX_Bucket_BucketGroupId ON Bucket(BucketGroupId);

-- BucketMovement table
CREATE TABLE IF NOT EXISTS BucketMovement (
    BucketMovementId UUID PRIMARY KEY,
    BucketId UUID NOT NULL,
    Amount DECIMAL(38, 2) NOT NULL,
    MovementDate DATE NOT NULL,
    --FOREIGN KEY (BucketId) REFERENCES Bucket(BucketId)
);

--CREATE INDEX IF NOT EXISTS IX_BucketMovement_BucketId ON BucketMovement(BucketId);

-- BucketRuleSet table
CREATE TABLE IF NOT EXISTS BucketRuleSet (
    BucketRuleSetId UUID PRIMARY KEY,
    Priority INTEGER NOT NULL,
    Name VARCHAR,
    TargetBucketId UUID NOT NULL,
    --FOREIGN KEY (TargetBucketId) REFERENCES Bucket(BucketId)
);

--CREATE INDEX IF NOT EXISTS IX_BucketRuleSet_TargetBucketId ON BucketRuleSet(TargetBucketId);

-- BucketVersion table
CREATE TABLE IF NOT EXISTS BucketVersion (
    BucketVersionId UUID PRIMARY KEY,
    BucketId UUID NOT NULL,
    Version INTEGER NOT NULL,
    BucketType INTEGER NOT NULL,
    BucketTypeXParam INTEGER NOT NULL,
    BucketTypeYParam DECIMAL(38, 2) NOT NULL,
    BucketTypeZParam DATE NOT NULL,
    Notes VARCHAR,
    ValidFrom DATE NOT NULL,
    --FOREIGN KEY (BucketId) REFERENCES Bucket(BucketId)
);

--CREATE INDEX IF NOT EXISTS IX_BucketVersion_BucketId ON BucketVersion(BucketId);

-- BudgetedTransaction table
CREATE TABLE IF NOT EXISTS BudgetedTransaction (
    BudgetedTransactionId UUID PRIMARY KEY,
    TransactionId UUID NOT NULL,
    BucketId UUID NOT NULL,
    Amount DECIMAL(38, 2) NOT NULL,
    --FOREIGN KEY (TransactionId) REFERENCES BankTransaction(TransactionId),
    --FOREIGN KEY (BucketId) REFERENCES Bucket(BucketId)
);

--CREATE INDEX IF NOT EXISTS IX_BudgetedTransaction_BucketId ON BudgetedTransaction(BucketId);
--CREATE INDEX IF NOT EXISTS IX_BudgetedTransaction_TransactionId ON BudgetedTransaction(TransactionId);

-- MappingRule table
CREATE TABLE IF NOT EXISTS MappingRule (
    MappingRuleId UUID PRIMARY KEY,
    BucketRuleSetId UUID NOT NULL,
    ComparisonField INTEGER NOT NULL,
    ComparisonType INTEGER NOT NULL,
    ComparisonValue VARCHAR NOT NULL,
    --FOREIGN KEY (BucketRuleSetId) REFERENCES BucketRuleSet(BucketRuleSetId)
);

--CREATE INDEX IF NOT EXISTS IX_MappingRule_BucketRuleSetId ON MappingRule(BucketRuleSetId);

-- Insert initial data
-- System BucketGroup
INSERT INTO BucketGroup (BucketGroupId, Name, Position)
VALUES ('00000000-0000-0000-0000-000000000001', 'System', 0);

-- System Buckets (Income and Transfer)
INSERT INTO Bucket (BucketId, Name, BucketGroupId, ColorCode, TextColorCode, ValidFrom, IsInactive, IsInactiveFrom, IsHiddenFromSummaries)
VALUES ('00000000-0000-0000-0000-000000000001', 'Income', '00000000-0000-0000-0000-000000000001', NULL, NULL, '1990-01-01', false, '9999-12-31', false);

INSERT INTO Bucket (BucketId, Name, BucketGroupId, ColorCode, TextColorCode, ValidFrom, IsInactive, IsInactiveFrom, IsHiddenFromSummaries)
VALUES ('00000000-0000-0000-0000-000000000002', 'Transfer', '00000000-0000-0000-0000-000000000001', NULL, NULL, '1990-01-01', false, '9999-12-31', true);

-- System BucketVersions
INSERT INTO BucketVersion (BucketVersionId, BucketId, Version, BucketType, BucketTypeXParam, BucketTypeYParam, BucketTypeZParam, Notes, ValidFrom)
VALUES ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 1, 1, 0, 0, '0001-01-01', NULL, '0001-01-01');

INSERT INTO BucketVersion (BucketVersionId, BucketId, Version, BucketType, BucketTypeXParam, BucketTypeYParam, BucketTypeZParam, Notes, ValidFrom)
VALUES ('00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000002', 1, 1, 0, 0, '0001-01-01', NULL, '0001-01-01');