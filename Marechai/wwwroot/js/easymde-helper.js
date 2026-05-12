// Helper to read the live value of an EasyMDE editor by walking the DOM.
//
// Why: PSC.Blazor.Components.MarkdownEditor's @bind-Value and GetValueAsync()
// can't be relied on inside MudDialog wrappers — the component lifecycle there
// produces a phantom MarkdownEditor instance whose JS bridge captures EasyMDE,
// while @ref captures a different (uninitialized) instance. Result: keystrokes
// fire SignalR traffic but never reach our component, GetValueAsync() returns
// null, and Submit() ships the unchanged InitialMarkdown.
//
// This helper reads CodeMirror.getValue() directly off the DOM, regardless of
// which (if any) PSC component instance is wired correctly.
window.marechaiReadEasyMdeValue = function (containerSelector) {
    try {
        var roots = containerSelector
            ? document.querySelectorAll(containerSelector + ' .EasyMDEContainer')
            : document.querySelectorAll('.EasyMDEContainer');
        if (!roots || roots.length === 0) return null;

        // Prefer the LAST container in the document — when a dialog opens on top
        // of a page that already has an EasyMDE elsewhere, the dialog's editor
        // will be the most recently mounted one.
        var root = roots[roots.length - 1];
        var cm   = root.querySelector('.CodeMirror');
        if (!cm || !cm.CodeMirror) return null;

        return cm.CodeMirror.getValue();
    } catch (e) {
        console.error('marechaiReadEasyMdeValue failed', e);
        return null;
    }
};
