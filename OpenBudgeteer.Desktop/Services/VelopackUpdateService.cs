using Microsoft.Extensions.Logging;
using OpenBudgeteer.Blazor.Common.Services;
using Velopack;
using Velopack.Sources;

namespace OpenBudgeteer.Desktop.Services;

/// <summary>
/// Updates the app using the Velopack packages attached to the GitHub Releases
/// </summary>
/// <remarks>
/// The update channel (e.g. linux-x64) is taken from the installed package, so each OS and architecture only receives
/// its own updates
/// </remarks>
public class VelopackUpdateService : IAppUpdateService
{
    private const string REPOSITORY_URL = "https://github.com/TheAxelander/OpenBudgeteer";
    private const string UPDATE_SOURCE_ENVIRONMENT_VARIABLE = "OPENBUDGETEER_UPDATE_SOURCE";

    public string? AvailableVersion => _updateInfo?.TargetFullRelease.Version.ToString();
    public int? DownloadProgress { get; private set; }

    public event EventHandler? StateChanged;

    private readonly UpdateManager _updateManager;
    private readonly ILogger<VelopackUpdateService> _logger;
    private readonly Lazy<Task> _checkForUpdatesTask;
    private UpdateInfo? _updateInfo;

    public VelopackUpdateService(UpdateManager updateManager, ILogger<VelopackUpdateService> logger)
    {
        _updateManager = updateManager;
        _logger = logger;
        _checkForUpdatesTask = new Lazy<Task>(CheckForUpdatesInternalAsync);
    }

    /// <summary>
    /// Creates the <see cref="UpdateManager"/> if the app is able to update itself
    /// </summary>
    /// <returns>UpdateManager or null if the app has not been installed via Velopack (e.g. dotnet run, Flatpak)</returns>
    public static UpdateManager? TryCreateUpdateManager()
    {
        // Flatpak updates are handled by Flatpak itself
        if (Environment.GetEnvironmentVariable("FLATPAK_ID") != null) return null;

        // Allows testing updates against a local folder or web server containing the output of "vpk pack"
        var testSource = Environment.GetEnvironmentVariable(UPDATE_SOURCE_ENVIRONMENT_VARIABLE);

        // Desktop App is in beta, so pre-releases are included
        var updateManager = string.IsNullOrEmpty(testSource)
            ? new UpdateManager(new GithubSource(REPOSITORY_URL, null, true))
            : new UpdateManager(testSource);
        return updateManager.IsInstalled ? updateManager : null;
    }

    public Task CheckForUpdatesAsync() => _checkForUpdatesTask.Value;

    private async Task CheckForUpdatesInternalAsync()
    {
        try
        {
            _updateInfo = await _updateManager.CheckForUpdatesAsync();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            // e.g. no internet connection or GitHub API rate limit, try again on next app start
            _logger.LogWarning(e, "Unable to check for updates");
        }
    }

    public async Task DownloadAndApplyUpdateAsync()
    {
        if (_updateInfo == null || DownloadProgress != null) return;

        try
        {
            DownloadProgress = 0;
            StateChanged?.Invoke(this, EventArgs.Empty);
            await _updateManager.DownloadUpdatesAsync(_updateInfo, progress =>
            {
                DownloadProgress = progress;
                StateChanged?.Invoke(this, EventArgs.Empty);
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to download update");
            DownloadProgress = null;
            StateChanged?.Invoke(this, EventArgs.Empty);
            throw;
        }

        _updateManager.ApplyUpdatesAndRestart(_updateInfo); // Exits the process
    }
}
