using System.Threading.Tasks;

namespace OpenBudgeteer.Core.Common.AppSettings;

public interface IAppSettingService
{
    public AppSettings CurrentSettings { get; }

    public Task InitializeAsync();
    public Task<AppSettings> GetSettingsAsync();
    public Task ApplySettingsAsync(AppSettings newSettings);
    public Task ResetSettingsAsync();
}
