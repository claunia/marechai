// software-promo-art-upload.js — XHR-based Software promo art batch upload helper for the
// collaborative-suggestion dialog.
//
// Differences from pending-cover.js:
//   * Multi-file: a single hidden <input type="file" multiple> drives N parallel uploads.
//   * Per-byte progress: XHR's upload.onprogress invokes a [JSInvokable] C# callback so
//     the dialog can render a real <MudProgressLinear> per file (not an indeterminate spinner).
//   * Cancellable: each in-flight upload is tracked by a stable client-generated guid hint
//     so the dialog can xhr.abort() when the user removes a file mid-upload.
//   * Used by: Marechai/Pages/Suggestions/SoftwarePromoArtSuggestionDialog.razor

window.MarechaiSoftwarePromoArtUpload = (function () {

    // Map: clientGuidHint (string) -> XMLHttpRequest. Lets the dialog cancel
    // in-flight uploads when the user removes a staged photo.
    const inFlight = new Map();

    /**
     * Drain the staged file selection from a hidden <input type="file" multiple>,
     * uploading each file in parallel via XHR with progress callbacks. Returns
     * synchronously after kicking off all uploads — completion is signalled via
     * the [JSInvokable] callbacks on dotNetRef.
     *
     * Required [JSInvokable] callbacks on dotNetRef:
     *   OnPhotoUploadStarted(clientGuid, fileName, sizeBytes)
     *   OnPhotoUploadProgress(clientGuid, percent)
     *   OnPhotoUploadCompleted(clientGuid, serverGuid, extension, sizeBytes)
     *   OnPhotoUploadFailed(clientGuid, errorMessage)
     *
     * @param {string} apiBaseUrl Absolute base URL of the API server.
     * @param {string} jwtToken   JWT bearer token (no "Bearer " prefix).
     * @param {number} softwareId      Parent Software id (sent as ?softwareId= query string).
     * @param {Element|string} fileInputElementOrId The <input type="file" multiple> element or its id.
     * @param {object} dotNetRef  DotNetObjectReference returned from C# DotNetObjectReference.Create(this).
     * @returns {{ accepted: number, rejected: number }}  How many files were accepted (uploads started)
     *          vs rejected by client-side guardrails (size/MIME); the input is then cleared.
     */
    function uploadSoftwarePromoArtImages(apiBaseUrl, jwtToken, softwareId, fileInputElementOrId, dotNetRef) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { accepted: 0, rejected: 0 };
        }

        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/software/promo-art/pending?softwareId=${encodeURIComponent(softwareId)}`;

        let accepted = 0;
        let rejected = 0;

        for (const file of Array.from(input.files)) {
            // Client-side guardrails — server enforces too.
            if (file.size > 50 * 1024 * 1024) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', cryptoRandomGuid(), `${file.name}: exceeds 50 MB limit`); }
                catch { /* ignored */ }
                continue;
            }
            if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', cryptoRandomGuid(), `${file.name}: unsupported type (allowed: JPG, PNG, WebP)`); }
                catch { /* ignored */ }
                continue;
            }

            const clientGuid = cryptoRandomGuid();

            // Tell C# the upload has started so it can render a placeholder card.
            try { dotNetRef.invokeMethodAsync('OnPhotoUploadStarted', clientGuid, file.name, file.size); }
            catch { /* ignored */ }

            // Kick off the XHR.
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
                    if (body && body.guid && body.extension) {
                        try { dotNetRef.invokeMethodAsync('OnPhotoUploadCompleted', clientGuid, body.guid, body.extension, file.size); }
                        catch { /* ignored */ }
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
                // Don't dispatch OnPhotoUploadFailed — the C# side initiated the abort and
                // already removed its UI state.
            };

            const fd = new FormData();
            fd.append('file', file, file.name);
            xhr.send(fd);

            accepted++;
        }

        // Reset the input so re-selecting the same file fires the change event again.
        try { input.value = ''; } catch { /* ignored on some browsers */ }

        return { accepted: accepted, rejected: rejected };
    }

    /**
     * Abort an in-flight upload by its clientGuidHint (returned by OnPhotoUploadStarted).
     * Idempotent — silently no-ops when the guid is unknown or the upload already finished.
     */
    function cancelUpload(clientGuid) {
        const xhr = inFlight.get(clientGuid);
        if (xhr) {
            try { xhr.abort(); } catch { /* ignored */ }
            inFlight.delete(clientGuid);
        }
    }

    /**
     * Number of in-flight uploads — useful for the dialog to gate the Submit button.
     */
    function inFlightCount() {
        return inFlight.size;
    }

    /**
     * Fetch a pending Software promo art image as a blob URL and assign it to the given <img>'s src.
     * Mirrors MarechaiPendingCover.applyPendingCoverImage but for the software-promo-art item folder.
     */
    async function applyPendingPhotoImage(apiBaseUrl, jwtToken, guid, imgElementOrId) {
        const img = (typeof imgElementOrId === 'string')
            ? document.getElementById(imgElementOrId)
            : imgElementOrId;
        if (!img) return { ok: false, status: 0, error: 'Image element not found.' };

        if (img.dataset && img.dataset.pendingPhotoUrl) {
            try { URL.revokeObjectURL(img.dataset.pendingPhotoUrl); } catch { /* ignored */ }
            delete img.dataset.pendingPhotoUrl;
        }

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/software/promo-art/pending/${encodeURIComponent(guid)}`;

        try {
            const resp = await fetch(url, { headers: { 'Authorization': `Bearer ${jwtToken}` } });
            if (!resp.ok) return { ok: false, status: resp.status, error: `HTTP ${resp.status}` };

            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            img.src = objUrl;
            if (img.dataset) img.dataset.pendingPhotoUrl = objUrl;
            return { ok: true, status: resp.status };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    /**
     * Open a pending Software promo art image in a new tab via a blob URL (admin lightbox).
     */
    async function openPendingPhotoInNewTab(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/software/promo-art/pending/${encodeURIComponent(guid)}`;
        try {
            const resp = await fetch(url, { headers: { 'Authorization': `Bearer ${jwtToken}` } });
            if (!resp.ok) return null;
            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            window.open(objUrl, '_blank');
            return objUrl;
        } catch {
            return null;
        }
    }

    function cryptoRandomGuid() {
        // crypto.randomUUID is available in modern browsers; fallback for older.
        if (window.crypto && typeof window.crypto.randomUUID === 'function') {
            return window.crypto.randomUUID();
        }
        // Fallback: low-quality but acceptable for a UI-only correlation id.
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    return {
        uploadSoftwarePromoArtImages,
        cancelUpload,
        inFlightCount,
        applyPendingPhotoImage,
        openPendingPhotoInNewTab
    };
})();
