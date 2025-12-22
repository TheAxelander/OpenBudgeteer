using System.Collections.ObjectModel;
using DuckDB.NET.Data;
using Microsoft.AspNetCore.Components;

namespace OpenBudgeteer.Photino.Pages;

public partial class Info : ComponentBase
{
    private record ThirdPartyInfo(string Name, string Url, string License, string LicenseUrl);

    [Inject] private DuckDBConnection DbConnection { get; set; } = null!;

    private string DatabasePath => DbConnection.Database;

    private ObservableCollection<ThirdPartyInfo> Software => new()
    {
        new("aspnet-api-versioning", "https://github.com/dotnet/aspnet-api-versioning", "MIT", "https://github.com/dotnet/aspnet-api-versioning/blob/main/LICENSE.txt"),
        new("Blazor-ApexCharts", "https://github.com/apexcharts/Blazor-ApexCharts", "MIT", "https://github.com/apexcharts/Blazor-ApexCharts/blob/master/LICENSE"),
        new("dotnet-env", "https://github.com/tonerdo/dotnet-env", "MIT", "https://github.com/tonerdo/dotnet-env/blob/master/LICENSE"),
        new("DuckDB.NET", "https://github.com/Giorgi/DuckDB.NET", "MIT", "https://github.com/Giorgi/DuckDB.NET/blob/master/LICENSE"),
        new("MudBlazor", "https://github.com/MudBlazor/MudBlazor", "MIT", "https://github.com/MudBlazor/MudBlazor/blob/dev/LICENSE"),
        new("Photino.NET", "https://github.com/tryphotino/photino.NET", "Apache-2.0", "https://github.com/tryphotino/photino.NET/blob/main/LICENSE"),
        new("TinyCsvParser", "https://github.com/TinyCsvParser/TinyCsvParser", "MIT", "https://github.com/TinyCsvParser/TinyCsvParser/blob/master/LICENSE"),
        new("xunit", "https://github.com/xunit/xunit", "Apache-2.0", "https://github.com/xunit/xunit/blob/main/LICENSE"),
        new(".NET runtime", "https://github.com/dotnet/runtime", "MIT", "https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"),
        new ThirdPartyInfo("Icon Images", string.Empty, "Icons by Icons8", "https://icons8.com/")
    };
}
