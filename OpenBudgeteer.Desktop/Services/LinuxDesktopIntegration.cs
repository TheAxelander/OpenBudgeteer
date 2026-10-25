using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenBudgeteer.Desktop.Services;

/// <summary>
/// Links the GTK window to the installed io.github.TheAxelander.OpenBudgeteer.desktop file
/// </summary>
/// <remarks>
/// Wayland ignores window icons set by the app. The compositor takes the icon from the .desktop file matching the
/// window's app_id, which GTK derives from the program name (otherwise the process name OpenBudgeteer.Desktop)
/// </remarks>
[SupportedOSPlatform("linux")]
public static class LinuxDesktopIntegration
{
    public const string APP_ID = "io.github.TheAxelander.OpenBudgeteer";

    [DllImport("libglib-2.0.so.0")]
    private static extern void g_set_prgname(string prgname);

    [DllImport("libgdk-3.so.0")]
    private static extern void gdk_set_program_class(string programClass);

    /// <summary>
    /// Sets the app_id (Wayland) and WM_CLASS (X11), must be called before GTK gets initialized by Photino
    /// </summary>
    public static void SetApplicationId()
    {
        try
        {
            g_set_prgname(APP_ID);
            gdk_set_program_class(APP_ID);
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
        {
            // Not critical, only the icon in window header and task bar is affected
            Console.WriteLine($"Unable to set application id: {e.Message}");
        }
    }
}
