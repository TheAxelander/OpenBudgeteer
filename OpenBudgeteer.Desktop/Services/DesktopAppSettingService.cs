using OpenBudgeteer.Core.Common.AppSettings;

namespace OpenBudgeteer.Desktop.Services;

public class DesktopAppSettingService : BaseAppSettingService
{
    private readonly ConfigFileService _settingHandler;

    public DesktopAppSettingService(ConfigFileService settingHandler) : base(settingHandler)
    {
        _settingHandler = settingHandler;
    }

    public override async Task ApplySettingsAsync(AppSettings newSettings)
    {
        await base.ApplySettingsAsync(newSettings);
        await _settingHandler.SaveAsync();
    }

    public override async Task ResetSettingsAsync()
    {
        await base.ResetSettingsAsync();
        await _settingHandler.SaveAsync();
    }
}
