using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Core.Data.Connection;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Info : ComponentBase
{
    private record ThirdPartyInfo(string Name, string Url, string License, string LicenseUrl);
    
    [Inject] DatabaseConnectionInfo DatabaseConnectionInfo { get; set; } = null!;
    
    private ObservableCollection<ThirdPartyInfo> Software => new()
    {
        new("aspnet-api-versioning", "https://github.com/dotnet/aspnet-api-versioning", "MIT", "https://github.com/dotnet/aspnet-api-versioning/blob/main/LICENSE.txt"),
        new("Blazor-ApexCharts", "https://github.com/apexcharts/Blazor-ApexCharts", "MIT", "https://github.com/apexcharts/Blazor-ApexCharts/blob/master/LICENSE"),
        new("Dapper", "https://github.com/DapperLib/Dapper", "Apache-2.0", "https://github.com/DapperLib/Dapper/blob/main/License.txt"),
        new("dotnet-env", "https://github.com/tonerdo/dotnet-env", "MIT", "https://github.com/tonerdo/dotnet-env/blob/master/LICENSE"),
        new("DuckDB.NET", "https://github.com/Giorgi/DuckDB.NET", "MIT", "https://github.com/Giorgi/DuckDB.NET/blob/develop/LICENSE"),
        new("efcore", "https://github.com/dotnet/efcore", "MIT", "https://github.com/dotnet/efcore/blob/main/LICENSE.txt"),
        new("efcore.pg", "https://github.com/npgsql/efcore.pg", "PostgreSQL License", "https://github.com/npgsql/efcore.pg/blob/main/LICENSE"),
        new("Microting.EntityFrameworkCore.MySql", "https://github.com/microting/Pomelo.EntityFrameworkCore.MySql", "MIT", "https://github.com/microting/Pomelo.EntityFrameworkCore.MySql/blob/master/LICENSE"),
        new("MudBlazor", "https://github.com/MudBlazor/MudBlazor", "MIT", "https://github.com/MudBlazor/MudBlazor/blob/dev/LICENSE"),
        new("Scalar", "https://github.com/scalar/scalar", "MIT", "https://github.com/scalar/scalar/blob/main/LICENSE"),
        new("StackExchange.Redis", "https://github.com/StackExchange/StackExchange.Redis/", "MIT", "https://github.com/StackExchange/StackExchange.Redis/blob/main/LICENSE"),
        new("TinyCsvParser", "https://github.com/TinyCsvParser/TinyCsvParser", "MIT", "https://github.com/TinyCsvParser/TinyCsvParser/blob/master/LICENSE"),
        new("xunit", "https://github.com/xunit/xunit", "Apache-2.0", "https://github.com/xunit/xunit/blob/main/LICENSE"),
        new(".NET runtime", "https://github.com/dotnet/runtime", "MIT", "https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"),
        new ThirdPartyInfo("Icon Images", string.Empty, "Icons by Icons8", "https://icons8.com/")
    };
}