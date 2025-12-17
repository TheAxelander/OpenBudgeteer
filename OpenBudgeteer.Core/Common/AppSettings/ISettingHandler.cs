using System.Threading.Tasks;

namespace OpenBudgeteer.Core.Common.AppSettings;

public interface ISettingHandler
{
    public Task<string> GetStringValueAsync(string key, string defaultValue);
    public Task<int> GetIntValueAsync(string key, int defaultValue);
    public Task<bool> GetBoolValueAsync(string key, bool defaultValue);

    public Task SetStringValueAsync(string key, string value);
    public Task SetIntValueAsync(string key, int value);
    public Task SetBoolValueAsync(string key, bool value);

    public Task DeleteKeyAsync(string key);
    public Task<bool> ContainsKeyAsync(string key);
}
