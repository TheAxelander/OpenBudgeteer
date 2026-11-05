using System.Diagnostics;
using Microsoft.JSInterop;

namespace OpenBudgeteer.Desktop.Services;

/// <summary>
/// Opens links pointing outside the app with the OS default handler (Browser, Mail client), called from external-links.js
/// </summary>
public static class ExternalLinkInterop
{
    private static readonly string[] AllowedSchemes = [Uri.UriSchemeHttp, Uri.UriSchemeHttps, Uri.UriSchemeMailto];

    [JSInvokable]
    public static void OpenExternalLink(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !AllowedSchemes.Contains(uri.Scheme)) return;

        // Uses ShellExecute on Windows, "open" on macOS and "xdg-open" on Linux (also forwarded to the portal in Flatpak)
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }
}
