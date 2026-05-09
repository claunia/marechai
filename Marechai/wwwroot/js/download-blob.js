// Saves an in-memory binary payload to disk via a temporary anchor element. Used by Marechai's
// /profile "Download my data" button so the GDPR Article 15 JSON dump downloads with a friendly
// filename instead of opening in the browser.
//
// Invoked from Blazor via:
//   await JS.InvokeVoidAsync("marechaiDownloadBlob.save", filename, mimeType, base64Bytes);
//
// We accept Base64 because Blazor Server's SignalR transport already encodes binary as Base64;
// encoding once on the wire and decoding once in the browser is faster than streaming raw bytes
// across SignalR.
window.marechaiDownloadBlob = {
    save: function (filename, mimeType, base64) {
        try {
            const binary = atob(base64);
            const len    = binary.length;
            const bytes  = new Uint8Array(len);
            for (let i = 0; i < len; i++) bytes[i] = binary.charCodeAt(i);

            const blob = new Blob([bytes], { type: mimeType || 'application/octet-stream' });
            const url  = URL.createObjectURL(blob);

            const a = document.createElement('a');
            a.href     = url;
            a.download = filename || 'download.bin';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);

            // Revoke after a tick so the browser has time to start the download.
            setTimeout(() => URL.revokeObjectURL(url), 1000);
        } catch (e) {
            console.error('marechaiDownloadBlob.save failed', e);
        }
    }
};
