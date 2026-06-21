// Keeps the top-bar search results popover anchored to the search box's own
// left edge while letting it stretch to the right edge of the viewport.
//
// MudAutocomplete's popover JS actively re-measures and rewrites the popover's
// inline `left`/`width` on open, on resize, and via its own collision-avoidance
// ("flip") logic — it does this regardless of any CSS we apply, so a CSS-only
// `left` override gets clobbered moments after render. We instead watch the
// popover element itself and correct its inline style back to "anchored to the
// search box, stretched to the right edge" every time MudBlazor changes it.
window.SearchDropdownPositioning = (function () {
    let attached = false;

    function reposition(anchor, popover) {
        const desiredLeft = anchor.getBoundingClientRect().left + "px";

        if (popover.style.left !== desiredLeft) popover.style.setProperty("left", desiredLeft, "important");
        if (popover.style.right !== "0px") popover.style.setProperty("right", "0px", "important");
        if (popover.style.width !== "auto") popover.style.setProperty("width", "auto", "important");
        if (popover.style.maxWidth !== "none") popover.style.setProperty("max-width", "none", "important");
    }

    function watch(anchor, popover) {
        reposition(anchor, popover);

        // Re-assert our positioning whenever MudBlazor's own JS touches the
        // popover's style/class (open, close, resize, flip recalculation).
        // Each call is a no-op once values already match, so this can't loop.
        new MutationObserver(() => reposition(anchor, popover))
            .observe(popover, { attributes: true, attributeFilter: ["style", "class"] });

        window.addEventListener("resize", () => reposition(anchor, popover));
    }

    return {
        attach: function (anchorId) {
            if (attached) return;

            const anchor = document.getElementById(anchorId);
            if (!anchor) return;

            const existing = document.querySelector(".search-popover-fullwidth");
            if (existing) {
                watch(anchor, existing);
                attached = true;
                return;
            }

            // The popover element doesn't exist until MudBlazor first renders
            // it (typically on first focus/keystroke), so wait for it to show
            // up in the DOM before attaching the real observer.
            const bodyObserver = new MutationObserver(() => {
                const popover = document.querySelector(".search-popover-fullwidth");
                if (!popover) return;

                bodyObserver.disconnect();
                watch(anchor, popover);
                attached = true;
            });
            bodyObserver.observe(document.body, { childList: true, subtree: true });
        }
    };
})();
