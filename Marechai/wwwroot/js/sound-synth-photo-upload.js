// sound-synth-photo-upload.js — XHR-based sound synth photo batch upload helper for the
// collaborative-suggestion dialog.
//
// Mirrors gpu-photo-upload.js verbatim with entity-name swap (GPU → sound synth).
// See gpu-photo-upload.js header for design notes.
//
// Used by: Marechai/Pages/Suggestions/SoundSynthPhotosSuggestionDialog.razor

window.MarechaiSoundSynthPhotoUpload = (function () {

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
     * @param {number} soundSynthId Parent sound synth id (sent as ?soundSynthId= query string).
     * @param {Element|string} fileInputElementOrId The <input type="file" multiple> element or its id.
     * @param {object} dotNetRef  DotNetObjectReference returned from C# DotNetObjectReference.Create(this).
     * @returns {{ accepted: number, rejected: number }}  How many files were accepted (uploads started)
     *          vs rejected by client-side guardrails (size/MIME); the input is then cleared.
     */
    function uploadSoundSynthPhotos(apiBaseUrl, jwtToken, soundSynthId, fileInputElementOrId, dotNetRef) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { accepted: 0, rejected: 0 };
        }

        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/sound-synths/photos/pending?soundSynthId=${encodeURIComponent(soundSynthId)}`;

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
                    let detail = `HTTP ${xhr.status}`;
                    try {
                        const body = xhr.response;
                        if (body && (body.detail || body.title)) {
                            detail = body.detail || body.title;
                        }
                    } catch { /* ignored */ }
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
     * Fetch a pending sound synth photo as a blob URL and assign it to the given <img>'s src.
     * Mirrors MarechaiPendingCover.applyPendingCoverImage but for the sound-synths item folder.
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

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/sound-synths/photos/pending/${encodeURIComponent(guid)}`;

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
     * Open a pending sound synth photo in a new tab via a blob URL (admin lightbox).
     */
    async function openPendingPhotoInNewTab(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/sound-synths/photos/pending/${encodeURIComponent(guid)}`;
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
        uploadSoundSynthPhotos,
        cancelUpload,
        inFlightCount,
        applyPendingPhotoImage,
        openPendingPhotoInNewTab
    };
})();
