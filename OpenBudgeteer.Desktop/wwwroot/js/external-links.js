// The WebView can't open external links on its own (e.g. target="_blank"), so pass them to the OS default handler
document.addEventListener('click', event => {
    const anchor = event.target.closest('a[href]');
    if (!anchor) return;

    const url = new URL(anchor.href, document.baseURI);
    if (url.origin === window.location.origin) return; // Internal Blazor navigation
    if (!['http:', 'https:', 'mailto:'].includes(url.protocol)) return;

    event.preventDefault();
    DotNet.invokeMethodAsync('OpenBudgeteer.Desktop', 'OpenExternalLink', url.href);
}, true);
