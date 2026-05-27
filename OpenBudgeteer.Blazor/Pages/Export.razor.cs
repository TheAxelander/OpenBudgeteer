using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Core.Data.Contracts.Services;
// Alias required: Account.razor in this namespace compiles to a class also named "Account",
// which shadows the entity type without this alias.
using BankAccount = OpenBudgeteer.Core.Data.Entities.Models.Account;

namespace OpenBudgeteer.Blazor.Pages;

/// <summary>
/// Code-behind for the Export page.
/// Provides filter state and download triggers for all three CSV export types.
/// Downloads are initiated by navigating to Minimal API endpoints in Program.cs
/// with forceLoad:true so the browser makes a real HTTP request and receives the file.
/// </summary>
public partial class Export : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    // ── Transaction filter state ──────────────────────────────────────────
    // MudDatePicker works with DateTime? so we convert to DateOnly? at download time.
    private DateTime? _txStartDateTime;
    private DateTime? _txEndDateTime;
    private BankAccount?  _txAccount;

    // ── Bucket filter state ───────────────────────────────────────────────
    private bool _includeInactive;

    // ── Bucket movement filter state ──────────────────────────────────────
    private DateTime? _mvStartDateTime;
    private DateTime? _mvEndDateTime;

    // Account list for the transaction account dropdown
    private IList<BankAccount> _accounts = new List<BankAccount>();

    protected override Task OnInitializedAsync()
    {
        // Load active accounts for the transaction filter dropdown
        _accounts = ServiceManager.AccountService.GetActiveAccounts().ToList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggers download of the transactions CSV.
    /// Navigates to the /export/transactions Minimal API endpoint which returns the file.
    /// </summary>
    private void DownloadTransactions()
    {
        var url = BuildUrl("/export/transactions",
            ("start",     _txStartDateTime.HasValue ? DateOnly.FromDateTime(_txStartDateTime.Value).ToString("yyyy-MM-dd") : null),
            ("end",       _txEndDateTime.HasValue   ? DateOnly.FromDateTime(_txEndDateTime.Value).ToString("yyyy-MM-dd")   : null),
            ("accountId", _txAccount?.Id.ToString()));

        // forceLoad bypasses Blazor routing so the browser makes a real HTTP GET
        // and the server responds with Content-Disposition:attachment (file download).
        NavigationManager.NavigateTo(url, forceLoad: true);
    }

    /// <summary>
    /// Triggers download of the buckets CSV.
    /// </summary>
    private void DownloadBuckets()
    {
        var url = BuildUrl("/export/buckets",
            ("includeInactive", _includeInactive ? "true" : null));

        NavigationManager.NavigateTo(url, forceLoad: true);
    }

    /// <summary>
    /// Triggers download of the bucket movements CSV.
    /// </summary>
    private void DownloadMovements()
    {
        var url = BuildUrl("/export/movements",
            ("start", _mvStartDateTime.HasValue ? DateOnly.FromDateTime(_mvStartDateTime.Value).ToString("yyyy-MM-dd") : null),
            ("end",   _mvEndDateTime.HasValue   ? DateOnly.FromDateTime(_mvEndDateTime.Value).ToString("yyyy-MM-dd")   : null));

        NavigationManager.NavigateTo(url, forceLoad: true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a URL from a base path and optional query parameters.
    /// Null-valued params are omitted so the endpoint receives clean input.
    /// </summary>
    private static string BuildUrl(string basePath, params (string Key, string? Value)[] queryParams)
    {
        var nonNull = queryParams.Where(p => p.Value is not null).ToList();
        if (!nonNull.Any()) return basePath;

        var sb = new StringBuilder(basePath).Append('?');
        sb.Append(string.Join("&", nonNull.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}")));
        return sb.ToString();
    }
}
