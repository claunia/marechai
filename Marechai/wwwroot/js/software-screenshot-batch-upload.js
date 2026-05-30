// software-screenshot-batch-upload.js — XHR-based admin batch upload helper for
// the Software Screenshots admin page.
//
// Differs from the suggestion-flow upload helper / single-upload:
//   * Hits /software/screenshots/admin/pending (Admin/UberAdmin only, 7-format whitelist).
//   * Server returns inline base64 thumbnail in the response — no second fetch needed.
//   * Per-file XHR with progress callbacks via DotNetObjectReference invokable methods.

window.MarechaiSoftwareScreenshotBatchUpload = (function () {

    // Map: clientGuid (string) -> XMLHttpRequest. Lets the dialog cancel
    // in-flight uploads when the user removes a staged image.
    const inFlight = new Map();

    /**
     * Drain the staged file selection from a hidden <input type="file" multiple>,
     * uploading each file in parallel via XHR with progress callbacks.
     *
     * Required [JSInvokable] callbacks on dotNetRef:
     *   OnScreenshotUploadStarted(clientGuid, fileName, sizeBytes)
     *   OnScreenshotUploadProgress(clientGuid, percent)
     *   OnScreenshotUploadCompleted(clientGuid, serverGuid, extension, thumbnailBase64, sizeBytes, width, height)
     *   OnScreenshotUploadFailed(clientGuid, errorMessage)
     *
     * @param {string} apiBaseUrl Absolute base URL of the API server.
     * @param {string} jwtToken   JWT bearer token (no "Bearer " prefix).
     * @param {number} softwareId Parent Software id (sent as ?softwareId= query string).
     * @param {Element|string} fileInputElementOrId The <input type="file" multiple> element or its id.
     * @param {object} dotNetRef  DotNetObjectReference returned from C# DotNetObjectReference.Create(this).
     * @param {number} remainingSlots How many more uploads the dialog can accept (cap = 50 minus already-staged).
     * @returns {{ accepted: number, rejected: number }}
     */
    function uploadBatch(apiBaseUrl, jwtToken, softwareId, fileInputElementOrId, dotNetRef, remainingSlots) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { accepted: 0, rejected: 0 };
        }

        const allowedTypes = [
            'image/jpeg',
            'image/png',
            'image/webp',
            'image/avif',
            'image/jxl',
            'image/bmp',
            'image/tiff'
        ];
        const allowedExts = ['.jpg', '.jpeg', '.png', '.webp', '.avif', '.jxl', '.bmp', '.tif', '.tiff'];
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/software/screenshots/admin/pending?softwareId=${encodeURIComponent(softwareId)}`;

        let accepted = 0;
        let rejected = 0;
        let slotsLeft = (typeof remainingSlots === 'number' && remainingSlots >= 0) ? remainingSlots : 50;

        for (const file of Array.from(input.files)) {
            if (slotsLeft <= 0) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnScreenshotUploadFailed', cryptoRandomGuid(), `${file.name}: batch cap reached (50)`); }
                catch { /* ignored */ }
                continue;
            }
            if (file.size > 50 * 1024 * 1024) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnScreenshotUploadFailed', cryptoRandomGuid(), `${file.name}: exceeds 50 MB limit`); }
                catch { /* ignored */ }
                continue;
            }

            const lowerName = (file.name || '').toLowerCase();
            const extOk = allowedExts.some(e => lowerName.endsWith(e));
            const mimeOk = !file.type || allowedTypes.includes(file.type.toLowerCase());
            if (!extOk && !mimeOk) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnScreenshotUploadFailed', cryptoRandomGuid(), `${file.name}: unsupported type (allowed: JPG, PNG, WebP, AVIF, JXL, BMP, TIFF)`); }
                catch { /* ignored */ }
                continue;
            }

            const clientGuid = cryptoRandomGuid();

            try { dotNetRef.invokeMethodAsync('OnScreenshotUploadStarted', clientGuid, file.name, file.size); }
            catch { /* ignored */ }

            const xhr = new XMLHttpRequest();
            inFlight.set(clientGuid, xhr);

            xhr.open('POST', url, true);
            xhr.setRequestHeader('Authorization', `Bearer ${jwtToken}`);
            xhr.responseType = 'json';

            xhr.upload.onprogress = function (ev) {
                if (!ev.lengthComputable) return;
                const pct = Math.min(100, Math.round((ev.loaded / ev.total) * 100));
                try { dotNetRef.invokeMethodAsync('OnScreenshotUploadProgress', clientGuid, pct); }
                catch { /* ignored */ }
            };

            xhr.onload = function () {
                inFlight.delete(clientGuid);

                if (xhr.status >= 200 && xhr.status < 300) {
                    const body = xhr.response;
                    if (body && body.id && body.extension && body.thumbnail_base64) {
                        try {
                            dotNetRef.invokeMethodAsync('OnScreenshotUploadCompleted',
                                clientGuid, body.id, body.extension, body.thumbnail_base64,
                                body.size_bytes || file.size, body.width || 0, body.height || 0);
                        } catch { /* ignored */ }
                    } else {
                        try { dotNetRef.invokeMethodAsync('OnScreenshotUploadFailed', clientGuid, `${file.name}: malformed server response`); }
                        catch { /* ignored */ }
                    }
                } else {
                    let detail = `HTTP ${xhr.status}`;
                    try {
                        const body = xhr.response;
                        if (body && (body.detail || body.title)) detail = body.detail || body.title;
                        else if (typeof body === 'string' && body.length > 0 && body.length < 300) detail = body;
                    } catch { /* ignored */ }
                    try { dotNetRef.invokeMethodAsync('OnScreenshotUploadFailed', clientGuid, `${file.name}: ${detail}`); }
                    catch { /* ignored */ }
                }
            };

            xhr.onerror = function () {
                inFlight.delete(clientGuid);
                try { dotNetRef.invokeMethodAsync('OnScreenshotUploadFailed', clientGuid, `${file.name}: network error`); }
                catch { /* ignored */ }
            };

            xhr.onabort = function () {
                inFlight.delete(clientGuid);
                // C# initiated; no callback dispatched.
            };

            const fd = new FormData();
            fd.append('file', file, file.name);
            xhr.send(fd);

            accepted++;
            slotsLeft--;
        }

        try { input.value = ''; } catch { /* ignored on some browsers */ }
        return { accepted: accepted, rejected: rejected };
    }

    /**
     * Abort an in-flight upload by clientGuid.
     */
    function cancelUpload(clientGuid) {
        const xhr = inFlight.get(clientGuid);
        if (xhr) {
            try { xhr.abort(); } catch { /* ignored */ }
            inFlight.delete(clientGuid);
        }
    }

    /**
     * Click a hidden file input on behalf of the dialog (the file picker dialog can only be
     * triggered from a synchronous DOM event, not after async work in Blazor).
     */
    function clickInput(id) {
        const el = document.getElementById(id);
        if (el) el.click();
    }

    function cryptoRandomGuid() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') {
            return window.crypto.randomUUID();
        }
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    return {
        uploadBatch,
        cancelUpload,
        clickInput
    };
})();
