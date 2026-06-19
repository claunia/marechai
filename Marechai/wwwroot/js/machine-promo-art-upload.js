// machine-promo-art-upload.js — XHR-based machine promo art batch upload helper for the
// collaborative-suggestion dialog.

window.MarechaiMachinePromoArtUpload = (function () {
    const inFlight = new Map();

    function uploadMachinePromoArtImages(apiBaseUrl, jwtToken, machineId, fileInputElementOrId, dotNetRef) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { accepted: 0, rejected: 0 };
        }

        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/machines/promo-art/pending?machineId=${encodeURIComponent(machineId)}`;

        let accepted = 0;
        let rejected = 0;

        for (const file of Array.from(input.files)) {
            if (file.size > 50 * 1024 * 1024) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', cryptoRandomGuid(), `${file.name}: exceeds 50 MB limit`); }
                catch { }
                continue;
            }

            if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
                rejected++;
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', cryptoRandomGuid(), `${file.name}: unsupported type (allowed: JPG, PNG, WebP)`); }
                catch { }
                continue;
            }

            const clientGuid = cryptoRandomGuid();

            try { dotNetRef.invokeMethodAsync('OnPhotoUploadStarted', clientGuid, file.name, file.size); }
            catch { }

            const xhr = new XMLHttpRequest();
            inFlight.set(clientGuid, xhr);

            xhr.open('POST', url, true);
            xhr.setRequestHeader('Authorization', `Bearer ${jwtToken}`);
            xhr.responseType = 'json';

            xhr.upload.onprogress = function (ev) {
                if (!ev.lengthComputable) return;
                const pct = Math.min(100, Math.round((ev.loaded / ev.total) * 100));
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadProgress', clientGuid, pct); }
                catch { }
            };

            xhr.onload = function () {
                inFlight.delete(clientGuid);

                if (xhr.status >= 200 && xhr.status < 300) {
                    const body = xhr.response;
                    if (body && body.guid && body.extension) {
                        try { dotNetRef.invokeMethodAsync('OnPhotoUploadCompleted', clientGuid, body.guid, body.extension, file.size); }
                        catch { }
                    } else {
                        try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', clientGuid, `${file.name}: malformed server response`); }
                        catch { }
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
                    } catch { }

                    if (!detail) {
                        try {
                            const raw = xhr.responseText;
                            if (raw && raw.length > 0) {
                                const stripped = raw.replace(/<[^>]+>/g, '').trim();
                                if (stripped.length > 0 && stripped.length < 500) detail = stripped;
                            }
                        } catch { }
                    }

                    detail = detail ? `${detail} (HTTP ${xhr.status})` : `HTTP ${xhr.status}`;
                    try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', clientGuid, `${file.name}: ${detail}`); }
                    catch { }
                }
            };

            xhr.onerror = function () {
                inFlight.delete(clientGuid);
                try { dotNetRef.invokeMethodAsync('OnPhotoUploadFailed', clientGuid, `${file.name}: network error`); }
                catch { }
            };

            xhr.onabort = function () {
                inFlight.delete(clientGuid);
            };

            const fd = new FormData();
            fd.append('file', file, file.name);
            xhr.send(fd);

            accepted++;
        }

        try { input.value = ''; } catch { }

        return { accepted: accepted, rejected: rejected };
    }

    function cancelUpload(clientGuid) {
        const xhr = inFlight.get(clientGuid);
        if (xhr) {
            try { xhr.abort(); } catch { }
            inFlight.delete(clientGuid);
        }
    }

    function inFlightCount() {
        return inFlight.size;
    }

    async function applyPendingPhotoImage(apiBaseUrl, jwtToken, guid, imgElementOrId) {
        const img = (typeof imgElementOrId === 'string')
            ? document.getElementById(imgElementOrId)
            : imgElementOrId;
        if (!img) return { ok: false, status: 0, error: 'Image element not found.' };

        if (img.dataset && img.dataset.pendingPhotoUrl) {
            try { URL.revokeObjectURL(img.dataset.pendingPhotoUrl); } catch { }
            delete img.dataset.pendingPhotoUrl;
        }

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/machines/promo-art/pending/${encodeURIComponent(guid)}`;

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

    async function openPendingPhotoInNewTab(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/machines/promo-art/pending/${encodeURIComponent(guid)}`;
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
        uploadMachinePromoArtImages,
        cancelUpload,
        inFlightCount,
        applyPendingPhotoImage,
        openPendingPhotoInNewTab
    };
})();
