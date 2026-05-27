using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.API;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Initialization;
using OpenBudgeteer.Core.Data.Services.EFCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi("v1.1", options =>
{
    options.AddDocumentTransformer<CustomDocumentTransformer>();
});
builder.Services.AddLogging(x => x
#if DEBUG
    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
#endif
    .AddConsole());
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IServiceManager, EFCoreServiceManager>(x => 
    new EFCoreServiceManager(
        x.GetRequiredService<IDbContextFactory<DatabaseContext>>(),
        x.GetRequiredService<ILoggerFactory>()));

var app = builder.Build();

var serviceManager = new EFCoreServiceManager(
    app.Services.GetRequiredService<IDbContextFactory<DatabaseContext>>(),
    app.Services.GetRequiredService<ILoggerFactory>());

#region AccountService

var account = app.MapGroup("/account").WithTags("account");
var accountService = serviceManager.AccountService;

// GET
account.MapGet("{id:guid}", (Guid id) => accountService.Get(id));
account.MapGet("/", () => accountService.GetAll());
account.MapGet("/activeAccounts", () => accountService.GetActiveAccounts());

// POST
account.MapPost( "/", (Account entity) => accountService.Create(entity));

// PATCH
account.MapPatch( "/", (Account entity) => accountService.Update(entity));

// DELETE
account.MapDelete( "/{id:guid}", (Guid id) => accountService.Delete(id));
account.MapDelete( "/close/{id:guid}", (Guid id) => accountService.CloseAccount(id));

#endregion

#region BankTransaction

var transaction = app.MapGroup("/transaction").WithTags("transaction");
var bankTransactionService = serviceManager.BankTransactionService;

// GET
transaction.MapGet("{id:guid}", (Guid id) => bankTransactionService.Get(id));
transaction.MapGet("/", (DateOnly? start, DateOnly? end, int? limit) => 
    bankTransactionService.GetAll(start, end, limit ?? 0));
transaction.MapGet("/fromAccount/{id:guid}", (Guid id, DateOnly? start, DateOnly? end, int? limit) => 
    bankTransactionService.GetFromAccount(id, start, end, limit ?? 0));
transaction.MapGet( "/withEntities/{id:guid}", (Guid id) => 
    bankTransactionService.GetWithEntities(id));

// POST
transaction.MapPost( "/", (BankTransaction entity) => bankTransactionService.Create(entity))
    .Accepts<BankTransaction>("application/json");
transaction.MapPost( "/withBucket", (BankTransaction entity) => bankTransactionService.Create(entity))
    .Accepts<BankTransaction>("application/json");

// PATCH
transaction.MapPatch( "/", (BankTransaction entity) => bankTransactionService.Update(entity))
    .Accepts<BankTransaction>("application/json");
transaction.MapPatch( "/withBucket", (BankTransaction entity) => bankTransactionService.Update(entity))
    .Accepts<BankTransaction>("application/json");

// DELETE
transaction.MapDelete( "/{id:guid}", (Guid id) => bankTransactionService.Delete(id));

#endregion

#region RecurringTransaction

var recurring = app.MapGroup("/recurring").WithTags("recurring");
var recurringTransactionService = serviceManager.RecurringBankTransactionService;

// GET
recurring.MapGet("{id:guid}", (Guid id) => recurringTransactionService.Get(id));
recurring.MapGet("/", () => recurringTransactionService.GetAllWithEntities());
recurring.MapGet( "/withEntities/{id:guid}", (Guid id) => 
    recurringTransactionService.GetWithEntities(id));
recurring.MapGet("/pendingTransactions", async (DateOnly yearMonth) => 
    await recurringTransactionService.GetPendingBankTransactionAsync(yearMonth));

// POST
recurring.MapPost( "/", (RecurringBankTransaction entity) => recurringTransactionService.Create(entity))
    .Accepts<RecurringBankTransaction>("application/json");
recurring.MapPost("/pendingTransactions", async (DateOnly yearMonth) => 
    await recurringTransactionService.CreatePendingBankTransactionAsync(yearMonth));

// PATCH
recurring.MapPatch( "/", (RecurringBankTransaction entity) => recurringTransactionService.Update(entity))
    .Accepts<RecurringBankTransaction>("application/json");

// DELETE
recurring.MapDelete( "/{id:guid}", (Guid id) => recurringTransactionService.Delete(id));

#endregion

#region Bucket

var bucket = app.MapGroup("/bucket").WithTags("bucket");
var bucketService = serviceManager.BucketService;

// GET
bucket.MapGet("{id:guid}", (Guid id) => bucketService.Get(id));
bucket.MapGet("/withLatestVersion/{id:guid}", (Guid id) => bucketService.GetWithLatestVersion(id));
bucket.MapGet("/", () => bucketService.GetAll());
bucket.MapGet("/withoutSystemBuckets", () => bucketService.GetAllWithoutSystemBuckets());
bucket.MapGet("/systemBuckets", () => bucketService.GetSystemBuckets());
bucket.MapGet("/activeBuckets", (DateOnly? validFrom) => 
    bucketService.GetActiveBuckets(validFrom ?? DateOnly.FromDateTime(DateTime.Today)));
bucket.MapGet("/getVersion/{id:guid}", (Guid id, DateOnly? yearMonth) => 
    bucketService.GetLatestVersion(id, yearMonth ?? DateOnly.FromDateTime(DateTime.Today)));
bucket.MapGet("/figures/{id:guid}", (Guid id, DateOnly? yearMonth) => 
    bucketService.GetFigures(id, yearMonth ?? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1)));
bucket.MapGet("/balance/{id:guid}", (Guid id, DateOnly? yearMonth) => 
    bucketService.GetBalance(id, yearMonth ?? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1)));
bucket.MapGet("/inOut/{id:guid}", (Guid id, DateOnly? yearMonth) => 
    bucketService.GetInAndOut(id, yearMonth ?? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1)));

// POST
bucket.MapPost( "/", (Bucket entity) => bucketService.Create(entity))
    .Accepts<Bucket>("application/json");

// PATCH
bucket.MapPatch( "/", (Bucket entity) => bucketService.Update(entity))
    .Accepts<Bucket>("application/json");

// DELETE
bucket.MapDelete( "/{id:guid}", (Guid id) => bucketService.Delete(id));
bucket.MapDelete( "/close/{id:guid}", (Guid id, DateOnly? yearMonth) => 
    bucketService.Close(id, yearMonth ?? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1)));

#endregion

#region BucketGroup

var group = app.MapGroup("/group").WithTags("group");
var groupService = serviceManager.BucketGroupService;

// GET
group.MapGet("{id:guid}", (Guid id) => groupService.GetWithBuckets(id));
group.MapGet("/", (bool full) => full ? groupService.GetAllFull() : groupService.GetAll());

// POST
group.MapPost( "/", (BucketGroup entity) => groupService.Create(entity))
    .Accepts<BucketGroup>("application/json");

// PATCH
group.MapPatch( "/", (BucketGroup entity) => groupService.Update(entity))
    .Accepts<BucketGroup>("application/json");
group.MapPatch( "/move/{id:guid}", (Guid id, int positions) => groupService.Move(id, positions));

// DELETE
group.MapDelete( "/{id:guid}", (Guid id) => groupService.Delete(id));

#endregion

#region RuleSet

var rules = app.MapGroup("/rules").WithTags("rules");
var ruleSetService = serviceManager.BucketRuleSetService;

// GET
rules.MapGet("{id:guid}", (Guid id) => ruleSetService.Get(id));
rules.MapGet("/", () => ruleSetService.GetAll());
rules.MapGet("/mappings/{id:guid}", (Guid id) => ruleSetService.GetMappingRules(id));

// POST
rules.MapPost( "/", (BucketRuleSet entity) => ruleSetService.Create(entity))
    .Accepts<BucketRuleSet>("application/json");

// PATCH
rules.MapPatch( "/", (BucketRuleSet entity) => ruleSetService.Update(entity))
    .Accepts<BucketRuleSet>("application/json");

// DELETE
rules.MapDelete( "/{id:guid}", (Guid id) => ruleSetService.Delete(id));

#endregion

#region BucketMovement

var movement = app.MapGroup("/movement").WithTags("movement");
var movementService = serviceManager.BucketMovementService;

// GET
movement.MapGet("{id:guid}", (Guid id) => movementService.Get(id));
movement.MapGet("/", (DateOnly? periodStart, DateOnly? periodEnd) => 
    movementService.GetAll(periodStart ?? DateOnly.MinValue, periodEnd ?? DateOnly.MaxValue));
movement.MapGet("/fromBucket/{id:guid}", (Guid id, DateOnly? periodStart, DateOnly? periodEnd) =>
    movementService.GetAllFromBucket(id, periodStart ?? DateOnly.MinValue, periodEnd ?? DateOnly.MaxValue));

// POST
movement.MapPost( "/", (BucketMovement entity) => movementService.Create(entity))
    .Accepts<BucketMovement>("application/json");

// PATCH
movement.MapPatch( "/", (BucketMovement entity) => movementService.Update(entity))
    .Accepts<BucketMovement>("application/json");

// DELETE
movement.MapDelete( "/{id:guid}", (Guid id) => movementService.Delete(id));

#endregion

#region ImportProfile

var import = app.MapGroup( "/import").WithTags("import");
var importProfileService = serviceManager.ImportProfileService;

// GET
import.MapGet("{id:guid}", (Guid id) => importProfileService.Get(id));
import.MapGet("/", () => importProfileService.GetAll());

// POST
import.MapPost( "/", (ImportProfile entity) => importProfileService.Create(entity))
    .Accepts<ImportProfile>("application/json");

// PATCH
import.MapPatch( "/", (ImportProfile entity) => importProfileService.Update(entity))
    .Accepts<ImportProfile>("application/json");

// DELETE
import.MapDelete( "/{id:guid}", (Guid id) => importProfileService.Delete(id));

#endregion

#region Export

// CSV Export endpoints — return file downloads for use in spreadsheet tools or AI analysis pipelines.
// All date params use ISO 8601 (yyyy-MM-dd). Omit start/end for an unbounded export.
var export = app.MapGroup("/export").WithTags("export");

// GET /export/transactions?start=2024-01-01&end=2024-12-31[&accountId=guid]
export.MapGet("/transactions", (DateOnly? start, DateOnly? end, Guid? accountId) =>
{
    var bytes = serviceManager.CsvExportService.ExportTransactions(start, end, accountId);
    var filename = $"transactions_{DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}.csv";
    return Results.File(bytes, "text/csv; charset=utf-8", filename);
});

// GET /export/buckets[?includeInactive=true]
export.MapGet("/buckets", (bool? includeInactive) =>
{
    var bytes = serviceManager.CsvExportService.ExportBuckets(includeInactive ?? false);
    var filename = $"buckets_{DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}.csv";
    return Results.File(bytes, "text/csv; charset=utf-8", filename);
});

// GET /export/movements?start=2024-01-01&end=2024-12-31
export.MapGet("/movements", (DateOnly? start, DateOnly? end) =>
{
    var bytes = serviceManager.CsvExportService.ExportBucketMovements(start, end);
    var filename = $"bucket_movements_{DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}.csv";
    return Results.File(bytes, "text/csv; charset=utf-8", filename);
});

#endregion

app.MapOpenApi("/openapi/{documentName}/openapi.json");
app.MapScalarApiReference("/api", options =>
{
    options.AddDocument("v1.1", "OpenBudgeteer API", routePattern: "openapi/{documentName}/openapi.json");
});
app.Run();
