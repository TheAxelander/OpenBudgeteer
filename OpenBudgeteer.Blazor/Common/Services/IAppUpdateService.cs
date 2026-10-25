using System;
using System.Threading.Tasks;

namespace OpenBudgeteer.Blazor.Common.Services;

/// <summary>
/// In-app updates, only registered by apps able to update themselves (Desktop App installed via Velopack)
/// </summary>
public interface IAppUpdateService
{
    /// <summary>
    /// Version of the available update, null if there is none (or not checked yet)
    /// </summary>
    public string? AvailableVersion { get; }

    /// <summary>
    /// Download progress in percent, null if no download is running
    /// </summary>
    public int? DownloadProgress { get; }

    public event EventHandler? StateChanged;

    /// <summary>
    /// Checks once per app start for an update, subsequent calls return the same result
    /// </summary>
    public Task CheckForUpdatesAsync();

    /// <summary>
    /// Downloads the available update, installs it and restarts the app
    /// </summary>
    public Task DownloadAndApplyUpdateAsync();
}
