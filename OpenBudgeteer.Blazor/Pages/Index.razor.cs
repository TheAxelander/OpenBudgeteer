using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Index : ComponentBase
{
    private const string NEWS_SOURCE = "https://raw.githubusercontent.com/TheAxelander/OpenBudgeteer-News/main/README.md";
    private MarkupString _convertedHtml;
    private bool _showErrorMessage;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await LoadNewsAsync();
    }

    private async Task LoadNewsAsync()
    {
        try
        {
            _showErrorMessage = false;
            var httpResponse = await new HttpClient().GetAsync(NEWS_SOURCE);
            httpResponse.EnsureSuccessStatusCode();
            _convertedHtml = new MarkupString(await httpResponse.Content.ReadAsStringAsync());
        }
        catch
        {
            _convertedHtml = new MarkupString();
            _showErrorMessage = true;
        }
    }
}