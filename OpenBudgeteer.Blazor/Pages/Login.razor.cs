using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Blazor.Common.Services;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Login : ComponentBase
{
    [Inject] private AppAuthenticationService AuthService { get; set; } = null!;
    [Inject] private IMudThemeService MudThemeService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "returnUrl")]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery(Name = "error")]
    private string? Error { get; set; }

    private string AppIconBackground => MudThemeService.CurrentThemeSetting.IsDarkMode ? "#616161" : "white";

    protected override void OnInitialized()
    {
        // If auth is disabled, redirect to Home page
        if (!AuthService.IsAuthenticationEnabled()) Navigation.NavigateTo("/", replace: true);
    }

    private string GetErrorMessage()
    {
        return Error switch
        {
            "invalid" => "Invalid username or password",
            "system" => "An error occurred during login. Please try again.",
            _ => string.Empty
        };
    }
}
