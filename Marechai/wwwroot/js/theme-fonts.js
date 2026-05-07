// Marechai theme web-font loader.
//
// Maintains a single <link data-theme-font="..."> element per active web-font URL inside <head>. Idempotent: callers
// pass the desired set of URLs and the helper diffs against the currently-attached set, appending new ones and
// removing obsolete ones. Designed to be called via Blazor JS interop from ThemeFontLoader.cs whenever the active
// theme changes.
window.marechaiThemeFonts = {
    apply(urls) {
        const desired = new Set(Array.isArray(urls) ? urls.filter(u => typeof u === 'string' && u.length > 0) : []);
        const head = document.head || document.getElementsByTagName('head')[0];
        if (!head) return;

        // Remove links that are no longer wanted.
        const existing = head.querySelectorAll('link[data-theme-font]');
        existing.forEach(link => {
            const url = link.getAttribute('data-theme-font');
            if (!desired.has(url)) {
                link.parentNode.removeChild(link);
            } else {
                desired.delete(url);
            }
        });

        // Append the remaining desired URLs.
        desired.forEach(url => {
            const link = document.createElement('link');
            link.setAttribute('rel', 'stylesheet');
            link.setAttribute('href', url);
            link.setAttribute('data-theme-font', url);
            head.appendChild(link);
        });
    }
};
