using System.Globalization;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenBudgeteer.API;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDatabase(Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new QueryStringApiVersionReader();
}).AddApiExplorer(options =>
{
    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
    // note: the specified format code will format the version as "'v'major[.minor][-status]"
    options.GroupNameFormat = "'v'VVV";
});
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IServiceManager, ServiceManager>(x => 
    new ServiceManager(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));

var app = builder.Build();

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(1, 0)
    // reporting api versions will return the headers
    // "api-supported-versions" and "api-deprecated-versions"
    .ReportApiVersions()
    .Build();
var serviceManager = new ServiceManager(app.Services.GetRequiredService<DbContextOptions<DatabaseContext>>());

#region AccountService

var account = app.MapGroup( "/account" )
    .WithApiVersionSet(versionSet);
var accountService = serviceManager.AccountService;

// GET
account.MapGet("{id:guid}", (Guid id) => accountService.Get(id))
    .MapToApiVersion(1,0);
account.MapGet("/", () => accountService.GetAll())
    .MapToApiVersion(1,0);
account.MapGet("/activeAccounts", () => accountService.GetActiveAccounts())
    .MapToApiVersion(1,0);

// POST
account.MapPost( "/", (Account entity) => accountService.Create(entity))
    .Accepts<Account>("application/json")
    .MapToApiVersion(1,0);

// PATCH
account.MapPatch( "/", (Account entity) => accountService.Update(entity))
    .Accepts<Account>("application/json")
    .MapToApiVersion(1,0);

// DELETE
account.MapDelete( "/{id:guid}", (Guid id) => accountService.Delete(id))
    .MapToApiVersion(1,0);
account.MapDelete( "/close/{id:guid}", (Guid id) => accountService.CloseAccount(id))
    .MapToApiVersion(1,0);

#endregion

#region BankTransaction

var transaction = app.MapGroup( "/transaction" )
    .WithApiVersionSet(versionSet);
var bankTransactionService = serviceManager.BankTransactionService;

// GET
transaction.MapGet("{id:guid}", (Guid id) => bankTransactionService.Get(id))
    .MapToApiVersion(1,0);
transaction.MapGet("/", (DateTime? start, DateTime? end, int? limit) => 
        bankTransactionService.GetAll(start, end, limit ?? 0))
    .MapToApiVersion(1,0);
transaction.MapGet("/fromAccount/{id:guid}", (Guid id, DateTime? start, DateTime? end, int? limit) => 
        bankTransactionService.GetFromAccount(id, start, end, limit ?? 0))
    .MapToApiVersion(1,0);
transaction.MapGet( "/withEntities/{id:guid}", (Guid id) => 
        bankTransactionService.GetWithEntities(id))
    .MapToApiVersion(1,0);

// POST
transaction.MapPost( "/", (BankTransaction entity) => bankTransactionService.Create(entity))
    .Accepts<BankTransaction>("application/json")
    .MapToApiVersion(1,0);
transaction.MapPost( "/withBucket", (BankTransaction entity) => 
        bankTransactionService.Create(entity))
    .Accepts<BankTransaction>("application/json")
    .MapToApiVersion(1,0);

// PATCH
transaction.MapPatch( "/", (BankTransaction entity) => bankTransactionService.Update(entity))
    .Accepts<BankTransaction>("application/json")
    .MapToApiVersion(1,0);
transaction.MapPatch( "/withBucket", (BankTransaction entity) => 
        bankTransactionService.Update(entity))
    .Accepts<BankTransaction>("application/json")
    .MapToApiVersion(1,0);

// DELETE
transaction.MapDelete( "/{id:guid}", (Guid id) => bankTransactionService.Delete(id))
    .MapToApiVersion(1,0);

#endregion

#region RecurringTransaction

var recurring = app.MapGroup( "/recurring" )
    .WithApiVersionSet(versionSet);
var recurringTransactionService = serviceManager.RecurringBankTransactionService;

// GET
recurring.MapGet("{id:guid}", (Guid id) => recurringTransactionService.Get(id))
    .MapToApiVersion(1,0);
recurring.MapGet("/", () => recurringTransactionService.GetAllWithEntities())
    .MapToApiVersion(1,0);
recurring.MapGet( "/withEntities/{id:guid}", (Guid id) => 
        recurringTransactionService.GetWithEntities(id))
    .MapToApiVersion(1,0);
recurring.MapGet("/pendingTransactions", async (DateTime yearMonth) => 
        await recurringTransactionService.GetPendingBankTransactionAsync(yearMonth))
    .MapToApiVersion(1,0);

// POST
recurring.MapPost( "/", (RecurringBankTransaction entity) => recurringTransactionService.Create(entity))
    .Accepts<RecurringBankTransaction>("application/json")
    .MapToApiVersion(1,0);
recurring.MapPost("/pendingTransactions", async (DateTime yearMonth) => 
        await recurringTransactionService.CreatePendingBankTransactionAsync(yearMonth))
    .MapToApiVersion(1,0);

// PATCH
recurring.MapPatch( "/", (RecurringBankTransaction entity) => recurringTransactionService.Update(entity))
    .Accepts<RecurringBankTransaction>("application/json")
    .MapToApiVersion(1,0);

// DELETE
recurring.MapDelete( "/{id:guid}", (Guid id) => recurringTransactionService.Delete(id))
    .MapToApiVersion(1,0);

#endregion

#region Bucket

var bucket = app.MapGroup( "/bucket" )
    .WithApiVersionSet(versionSet);
var bucketService = serviceManager.BucketService;

// GET
bucket.MapGet("{id:guid}", (Guid id) => bucketService.Get(id))
    .MapToApiVersion(1,0);
bucket.MapGet("/withLatestVersion/{id:guid}", (Guid id) => bucketService.GetWithLatestVersion(id))
    .MapToApiVersion(1,0);
bucket.MapGet("/", () => bucketService.GetAll())
    .MapToApiVersion(1,0);
bucket.MapGet("/systemBuckets", () => bucketService.GetSystemBuckets())
    .MapToApiVersion(1,0);
bucket.MapGet("/activeBuckets", (DateTime? validFrom) => bucketService.GetActiveBuckets(validFrom ?? DateTime.Now))
    .MapToApiVersion(1,0);
bucket.MapGet("/getVersion/{id:guid}", (Guid id, DateTime? yearMonth) => bucketService.GetLatestVersion(id, yearMonth ?? DateTime.Now))
    .MapToApiVersion(1,0);
bucket.MapGet("/figures/{id:guid}", (Guid id, DateTime? yearMonth) => 
        bucketService.GetFigures(id, yearMonth ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)))
    .MapToApiVersion(1,0);
bucket.MapGet("/balance/{id:guid}", (Guid id, DateTime? yearMonth) => 
        bucketService.GetBalance(id, yearMonth ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)))
    .MapToApiVersion(1,0);
bucket.MapGet("/inOut/{id:guid}", (Guid id, DateTime? yearMonth) => 
        bucketService.GetInAndOut(id, yearMonth ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)))
    .MapToApiVersion(1,0);

// POST
bucket.MapPost( "/", (Bucket entity) => bucketService.Create(entity))
    .Accepts<Bucket>("application/json")
    .MapToApiVersion(1,0);

// PATCH
bucket.MapPatch( "/", (Bucket entity) => bucketService.Update(entity))
    .Accepts<Bucket>("application/json")
    .MapToApiVersion(1,0);

// DELETE
bucket.MapDelete( "/{id:guid}", (Guid id) => bucketService.Delete(id))
    .MapToApiVersion(1,0);
bucket.MapDelete( "/close/{id:guid}", (Guid id, DateTime? yearMonth) => 
        bucketService.Close(id, yearMonth ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)))
    .MapToApiVersion(1,0);

#endregion

#region BucketGroup

var group = app.MapGroup( "/group" )
    .WithApiVersionSet(versionSet);
var groupService = serviceManager.BucketGroupService;

// GET
group.MapGet("{id:guid}", (Guid id) => groupService.GetWithBuckets(id))
    .MapToApiVersion(1,0);
group.MapGet("/", (bool full) => full ? groupService.GetAllFull() : groupService.GetAll())
    .MapToApiVersion(1,0);

// POST
group.MapPost( "/", (BucketGroup entity) => groupService.Create(entity))
    .Accepts<BucketGroup>("application/json")
    .MapToApiVersion(1,0);

// PATCH
group.MapPatch( "/", (BucketGroup entity) => groupService.Update(entity))
    .Accepts<BucketGroup>("application/json")
    .MapToApiVersion(1,0);
group.MapPatch( "/move/{id:guid}", (Guid id, int positions) => groupService.Move(id, positions))
    .MapToApiVersion(1,0);

// DELETE
group.MapDelete( "/{id:guid}", (Guid id) => groupService.Delete(id))
    .MapToApiVersion(1,0);

#endregion

#region RuleSet

var rules = app.MapGroup( "/rules" )
    .WithApiVersionSet(versionSet);
var ruleSetService = serviceManager.BucketRuleSetService;

// GET
rules.MapGet("{id:guid}", (Guid id) => ruleSetService.Get(id))
    .MapToApiVersion(1,0);
rules.MapGet("/", () => ruleSetService.GetAll())
    .MapToApiVersion(1,0);
rules.MapGet("/mappings/{id:guid}", (Guid id) => ruleSetService.GetMappingRules(id))
    .MapToApiVersion(1,0);

// POST
rules.MapPost( "/", (BucketRuleSet entity) => ruleSetService.Create(entity))
    .Accepts<BucketRuleSet>("application/json")
    .MapToApiVersion(1,0);

// PATCH
rules.MapPatch( "/", (BucketRuleSet entity) => ruleSetService.Update(entity))
    .Accepts<BucketRuleSet>("application/json")
    .MapToApiVersion(1,0);

// DELETE
rules.MapDelete( "/{id:guid}", (Guid id) => ruleSetService.Delete(id))
    .MapToApiVersion(1,0);

#endregion

#region BucketMovement

var movement = app.MapGroup( "/movement" )
    .WithApiVersionSet(versionSet);
var movementService = serviceManager.BucketMovementService;

// GET
movement.MapGet("{id:guid}", (Guid id) => movementService.Get(id))
    .MapToApiVersion(1,0);
movement.MapGet("/", (DateTime? periodStart, DateTime? periodEnd) => 
        movementService.GetAll(periodStart ?? DateTime.MinValue, periodEnd ?? DateTime.MaxValue))
    .MapToApiVersion(1,0);
movement.MapGet("/fromBucket/{id:guid}", (Guid id, DateTime? periodStart, DateTime? periodEnd) => 
        movementService.GetAllFromBucket(id, periodStart ?? DateTime.MinValue, periodEnd ?? DateTime.MaxValue))
    .MapToApiVersion(1,0);

// POST
movement.MapPost( "/", (BucketMovement entity) => movementService.Create(entity))
    .Accepts<BucketMovement>("application/json")
    .MapToApiVersion(1,0);

// PATCH
movement.MapPatch( "/", (BucketMovement entity) => movementService.Update(entity))
    .Accepts<BucketMovement>("application/json")
    .MapToApiVersion(1,0);

// DELETE
movement.MapDelete( "/{id:guid}", (Guid id) => movementService.Delete(id))
    .MapToApiVersion(1,0);

#endregion

#region ImportProfile

var import = app.MapGroup( "/import" )
    .WithApiVersionSet(versionSet);
var importProfileService = serviceManager.ImportProfileService;

// GET
import.MapGet("{id:guid}", (Guid id) => importProfileService.Get(id))
    .MapToApiVersion(1,0);
import.MapGet("/", () => importProfileService.GetAll())
    .MapToApiVersion(1,0);

// POST
import.MapPost( "/", (ImportProfile entity) => importProfileService.Create(entity))
    .Accepts<ImportProfile>("application/json")
    .MapToApiVersion(1,0);

// PATCH
import.MapPatch( "/", (ImportProfile entity) => importProfileService.Update(entity))
    .Accepts<ImportProfile>("application/json")
    .MapToApiVersion(1,0);

// DELETE
import.MapDelete( "/{id:guid}", (Guid id) => importProfileService.Delete(id))
    .MapToApiVersion(1,0);

#endregion

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();

        // build a swagger endpoint for each discovered API version
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });
//}

app.Run();
