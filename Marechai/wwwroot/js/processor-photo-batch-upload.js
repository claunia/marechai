// processor-photo-batch-upload.js — XHR-based admin batch upload helper for the
// Processor Photos admin page.
//
// Differs from processor-photo-upload.js (suggestions) / single-upload:
//   * Hits /processors/photos/admin/pending (Admin/UberAdmin only, 7-format whitelist).
//   * Server returns inline base64 thumbnail in the response — no second fetch needed.
//   * Per-file XHR with progress callbacks via DotNetObjectReference invokable methods.

window.MarechaiProcessorPhotoBatchUpload = (function () {

    // Map: clientGuid (string) -> XMLHttpRequest. Lets the dialog cancel
    // in-flight uploads when the user removes a staged image.
    const inFlight = new Map();

    /**
     * Drain the staged file selection from a hidden <input type="file" multiple>,
     * uploading each file in parallel via XHR with progress callbacks.
     *
     * Required [JSInvokable] callbacks on dotNetRef:
     *   OnPhotoUploadStarted(clientGuid, fileName, sizeBytes)
     *   OnPhotoUploadProgress(clientGuid, percent)
     *   OnPhotoUploadCompleted(clientGuid, serverGuid, extension, thumbnailBase64, sizeBytes, width, height)
     *   OnPhotoUploadFailed(clientGuid, errorMessage)
     *
     * @param {string} apiBaseUrl Absolute base URL of the API server.
     * @param {string} jwtToken   JWT bearer token (no "Bearer " prefix).
     * @param {number} processorId Parent Processor id (sent as ?processorId= query string).
     * @param {Element|string} fileInputElementOrId The <input type="file" multiple> element or its id.
     * @param {object} dotNetRef  DotNetObjectReference returned from C# DotNetObjectReference.Create(this).
     * @param {number} remainingSlots How many more uploads the dialog can accept (cap = 25 minus already-staged).
     * @returns {{ accepted: number, rejected: number }}
     */
    function uploadBatch(apiBaseUrl, jwtToken, processorId, fileInputElementOrId, dotNetRef, remainingSlots) {
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
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/processors/photos/admin/pending?processorId=${encodeURIComponent(processorId)}`;

        let accepted = 0;
        let rejected = 0;
        let slotsLeft = (typeof remainingSlots === 'number' && remainingSlots >= 0) ? remainingSlots : 25;

        for (const file of Array.from(input.files)) {
            if (slotsLeft <= 0) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', cryptoRandomGuid(), `${file.name}: batch cap reached (25)`); }
                catch { /* ignored */ }
                continue;
            }
            if (file.size > 50 * 1024 * 1024) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', cryptoRandomGuid(), `${file.name}: exceeds 50 MB limit`); }
                catch { /* ignored */ }
                continue;
            }

            const lowerName = (file.name || '').toLowerCase();
            const extOk = allowedExts.some(e => lowerName.endsWith(e));
            const mimeOk = !file.type || allowedTypes.includes(file.type.toLowerCase());
            if (!extOk && !mimeOk) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', cryptoRandomGuid(), `${file.name}: unsupported type (allowed: JPG, PNG, WebP, AVIF, JXL, BMP, TIFF)`); }
                catch { /* ignored */ }
                continue;
            }

            const clientGuid = cryptoRandomGuid();

            try { dotNetRef.invokeMethodAsync('OnPhotoUploadStarted', clientGuid, file.name, file.size); }
            catch { /* ignored */ }

            const xhr = new XMLHttpRequest();
            inFlight.set(clientGuid, xhr);

            xhr.open('POST', url, true);
            xhr.setRequestHeader('Authorization', `Bearer ${jwtToken}`);
            xhr.responseType = 'json';

            xhr.upload.onprogress = function (ev) {
                if (!ev.lengthComputable) return;
                const pct = Math.min(100, Math.round((ev.loaded / ev.total) * 100));
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadProgress', clientGuid, pct); }
                catch { /* ignored */ }
            };

            xhr.onload = function () {
                inFlight.delete(clientGuid);

                if (xhr.status >= 200 && xhr.status < 300) {
                    const body = xhr.response;
                    if (body && body.id && body.extension && body.thumbnail_base64) {
                        try {
                            dotNetRef.invokeMethodAsync('OnPhotoUploadCompleted',
                                clientGuid, body.id, body.extension, body.thumbnail_base64,
                                body.size_bytes || file.size, body.width || 0, body.height || 0);
                        } catch { /* ignored */ }
                    } else {
                        try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', clientGuid, `${file.name}: malformed server response`); }
                        catch { /* ignored */ }
                    }
                } else {
                    let detail = '';
                    try {
                        const body = xhr.response;
                        if (body && typeof body === 'object') {
                            detail = body.detail || body.title || body.error || body.message || '';
                        } else if (typeof body === 'string' && body.length > 0) {
                            detail = body;
                        }
                    } catch { /* ignored */ }
                    if (!detail) {
                        try {
                            const raw = xhr.responseText;
                            if (raw && raw.length > 0) {
                                const stripped = raw.replace(/<[^>]+>/g, '').trim();
                                if (stripped.length > 0 && stripped.length < 500) detail = stripped;
                            }
                        } catch { /* ignored */ }
                    }
                    detail = detail ? `${detail} (HTTP ${xhr.status})` : `HTTP ${xhr.status}`;
                    try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', clientGuid, `${file.name}: ${detail}`); }
                    catch { /* ignored */ }
                }
            };

            xhr.onerror = function () {
                inFlight.delete(clientGuid);
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', clientGuid, `${file.name}: network error`); }
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
