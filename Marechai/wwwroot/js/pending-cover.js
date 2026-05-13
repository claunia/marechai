// pending-cover.js — direct browser → API helper for collaborator-uploaded
// pending images (book covers today, magazine issue covers next).
//
// Avoids the Blazor SignalR double-hop: the browser uploads straight to
// the Marechai.Server API endpoint, with the JWT in an Authorization header
// (passed in from C# via the existing TokenProvider). The same helper is
// used for displaying the pending image preview, since <img src> can't
// carry an Authorization header — we fetch as a blob and create an object URL.

window.MarechaiPendingCover = (function () {
    /**
     * Upload a single file to the API's pending-cover endpoint.
     *
     * @param {string} apiBaseUrl  Absolute base URL of the API server (e.g. http://localhost:5023).
     * @param {string} jwtToken    JWT bearer token (no "Bearer " prefix).
     * @param {number} bookId      Target book id.
     * @param {Element|string} fileInputElementOrId  The <input type="file"> element or its id.
     * @returns {Promise<{ok: boolean, status: number, guid?: string, extension?: string, error?: string}>}
     */
    async function uploadBookCover(apiBaseUrl, jwtToken, bookId, fileInputElementOrId) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { ok: false, status: 0, error: 'No file selected.' };
        }

        const file = input.files[0];

        // Browser-side guardrails (server enforces too).
        if (file.size > 50 * 1024 * 1024) {
            return { ok: false, status: 0, error: 'File exceeds 50 MB limit.' };
        }
        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
            return { ok: false, status: 0, error: 'Unsupported file type. Allowed: JPG, PNG, WebP.' };
        }

        const fd = new FormData();
        fd.append('file', file, file.name);

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/books/${encodeURIComponent(bookId)}/cover/pending`;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${jwtToken}` },
                body: fd
            });

            if (!resp.ok) {
                let detail = '';
                try {
                    const body = await resp.text();
                    // ProblemDetails JSON or plain text — surface either form.
                    try {
                        const j = JSON.parse(body);
                        detail = j.detail || j.title || body;
                    } catch { detail = body; }
                } catch { /* ignored */ }
                return { ok: false, status: resp.status, error: detail || `HTTP ${resp.status}` };
            }

            const json = await resp.json();
            return { ok: true, status: resp.status, guid: json.guid, extension: json.extension };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    /**
     * Upload a single file to the API's NEW-book pending-cover endpoint.
     * Used by the "Suggest new book" addition-mode dialog where the book id doesn't
     * exist yet. Server stores the sidecar with EntityId=0 and enforces the
     * one-pending-per-uploader rule on that surrogate id.
     *
     * @param {string} apiBaseUrl  Absolute base URL of the API server.
     * @param {string} jwtToken    JWT bearer token (no "Bearer " prefix).
     * @param {Element|string} fileInputElementOrId  The <input type="file"> element or its id.
     * @returns {Promise<{ok: boolean, status: number, guid?: string, extension?: string, error?: string}>}
     */
    async function uploadNewBookCover(apiBaseUrl, jwtToken, fileInputElementOrId) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { ok: false, status: 0, error: 'No file selected.' };
        }

        const file = input.files[0];

        if (file.size > 50 * 1024 * 1024) {
            return { ok: false, status: 0, error: 'File exceeds 50 MB limit.' };
        }
        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
            return { ok: false, status: 0, error: 'Unsupported file type. Allowed: JPG, PNG, WebP.' };
        }

        const fd = new FormData();
        fd.append('file', file, file.name);

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/books/cover/pending/new`;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${jwtToken}` },
                body: fd
            });

            if (!resp.ok) {
                let detail = '';
                try {
                    const body = await resp.text();
                    try {
                        const j = JSON.parse(body);
                        detail = j.detail || j.title || body;
                    } catch { detail = body; }
                } catch { /* ignored */ }
                return { ok: false, status: resp.status, error: detail || `HTTP ${resp.status}` };
            }

            const json = await resp.json();
            return { ok: true, status: resp.status, guid: json.guid, extension: json.extension };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    /**
     * Delete a pending cover via the API.
     */
    async function deletePendingCover(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/books/cover/pending/${encodeURIComponent(guid)}`;
        try {
            const resp = await fetch(url, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            return { ok: resp.ok, status: resp.status };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    /**
     * Fetch a pending cover as a blob and apply it as a data URL to an <img> element.
     * Required because <img src> can't include an Authorization header. The helper
     * also revokes any previous object URL on the same element to avoid leaks.
     *
     * @returns {Promise<{ok: boolean, status: number, error?: string}>}
     */
    async function applyPendingCoverImage(apiBaseUrl, jwtToken, guid, imgElementOrId) {
        const img = (typeof imgElementOrId === 'string')
            ? document.getElementById(imgElementOrId)
            : imgElementOrId;
        if (!img) return { ok: false, status: 0, error: 'Image element not found.' };

        // Revoke any previous blob URL we set on this element.
        if (img.dataset && img.dataset.pendingCoverUrl) {
            try { URL.revokeObjectURL(img.dataset.pendingCoverUrl); } catch { /* ignored */ }
            delete img.dataset.pendingCoverUrl;
        }

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/books/cover/pending/${encodeURIComponent(guid)}`;

        try {
            const resp = await fetch(url, {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            if (!resp.ok) {
                return { ok: false, status: resp.status, error: `HTTP ${resp.status}` };
            }
            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            img.src = objUrl;
            if (img.dataset) img.dataset.pendingCoverUrl = objUrl;
            return { ok: true, status: resp.status };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    /**
     * Open a pending cover in a new tab/window via a blob URL (lightbox-style preview
     * for the admin review queue). Returns the object URL so the caller can later
     * revoke it; null on failure.
     */
    async function openPendingCoverInNewTab(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/books/cover/pending/${encodeURIComponent(guid)}`;
        try {
            const resp = await fetch(url, {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            if (!resp.ok) return null;
            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            window.open(objUrl, '_blank', 'noopener');
            // Revoke after a short delay so the new tab has time to grab it.
            setTimeout(() => { try { URL.revokeObjectURL(objUrl); } catch { /* ignored */ } }, 60000);
            return objUrl;
        } catch {
            return null;
        }
    }

    /**
     * Programmatic file picker click — lets a Blazor component trigger the native
     * file selector without rendering a visible <input type="file"> directly.
     */
    function clickInput(elementOrId) {
        const el = (typeof elementOrId === 'string') ? document.getElementById(elementOrId) : elementOrId;
        if (el) el.click();
    }

    // ─── MagazineIssue counterparts ───
    // Same shape as the Book functions above, but post to the
    // /magazines/issues/{id}/cover/pending endpoint and read/delete via
    // /magazines/issues/cover/pending/{guid}. We keep these as separate functions
    // (rather than parameterising the URL) so each call site is grep-friendly and
    // the existing Book wiring isn't perturbed.

    async function uploadMagazineIssueCover(apiBaseUrl, jwtToken, issueId, fileInputElementOrId) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { ok: false, status: 0, error: 'No file selected.' };
        }

        const file = input.files[0];

        if (file.size > 50 * 1024 * 1024) {
            return { ok: false, status: 0, error: 'File exceeds 50 MB limit.' };
        }
        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
            return { ok: false, status: 0, error: 'Unsupported file type. Allowed: JPG, PNG, WebP.' };
        }

        const fd = new FormData();
        fd.append('file', file, file.name);

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/magazines/issues/${encodeURIComponent(issueId)}/cover/pending`;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${jwtToken}` },
                body: fd
            });

            if (!resp.ok) {
                let detail = '';
                try {
                    const body = await resp.text();
                    try {
                        const j = JSON.parse(body);
                        detail = j.detail || j.title || body;
                    } catch { detail = body; }
                } catch { /* ignored */ }
                return { ok: false, status: resp.status, error: detail || `HTTP ${resp.status}` };
            }

            const json = await resp.json();
            return { ok: true, status: resp.status, guid: json.guid, extension: json.extension };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    /**
     * Upload a pending cover for a brand-new magazine issue (no issueId yet) to the
     * /magazines/issues/cover/pending/new endpoint. Mirror of uploadNewBookCover.
     * Used by the addition-mode (entityId=null) MagazineIssue suggestion dialog.
     *
     * @param {string} apiBaseUrl
     * @param {string} jwtToken
     * @param {Element|string} fileInputElementOrId
     * @returns {Promise<{ok: boolean, status: number, guid?: string, extension?: string, error?: string}>}
     */
    async function uploadNewMagazineIssueCover(apiBaseUrl, jwtToken, fileInputElementOrId) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { ok: false, status: 0, error: 'No file selected.' };
        }

        const file = input.files[0];

        if (file.size > 50 * 1024 * 1024) {
            return { ok: false, status: 0, error: 'File exceeds 50 MB limit.' };
        }
        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
            return { ok: false, status: 0, error: 'Unsupported file type. Allowed: JPG, PNG, WebP.' };
        }

        const fd = new FormData();
        fd.append('file', file, file.name);

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/magazines/issues/cover/pending/new`;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${jwtToken}` },
                body: fd
            });

            if (!resp.ok) {
                let detail = '';
                try {
                    const body = await resp.text();
                    try {
                        const j = JSON.parse(body);
                        detail = j.detail || j.title || body;
                    } catch { detail = body; }
                } catch { /* ignored */ }
                return { ok: false, status: resp.status, error: detail || `HTTP ${resp.status}` };
            }

            const json = await resp.json();
            return { ok: true, status: resp.status, guid: json.guid, extension: json.extension };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    async function deletePendingMagazineIssueCover(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/magazines/issues/cover/pending/${encodeURIComponent(guid)}`;
        try {
            const resp = await fetch(url, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            return { ok: resp.ok, status: resp.status };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    async function applyPendingMagazineIssueCoverImage(apiBaseUrl, jwtToken, guid, imgElementOrId) {
        const img = (typeof imgElementOrId === 'string')
            ? document.getElementById(imgElementOrId)
            : imgElementOrId;
        if (!img) return { ok: false, status: 0, error: 'Image element not found.' };

        if (img.dataset && img.dataset.pendingCoverUrl) {
            try { URL.revokeObjectURL(img.dataset.pendingCoverUrl); } catch { /* ignored */ }
            delete img.dataset.pendingCoverUrl;
        }

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/magazines/issues/cover/pending/${encodeURIComponent(guid)}`;

        try {
            const resp = await fetch(url, {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            if (!resp.ok) {
                return { ok: false, status: resp.status, error: `HTTP ${resp.status}` };
            }
            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            img.src = objUrl;
            if (img.dataset) img.dataset.pendingCoverUrl = objUrl;
            return { ok: true, status: resp.status };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    async function openPendingMagazineIssueCoverInNewTab(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/magazines/issues/cover/pending/${encodeURIComponent(guid)}`;
        try {
            const resp = await fetch(url, {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            if (!resp.ok) return null;
            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            window.open(objUrl, '_blank', 'noopener');
            setTimeout(() => { try { URL.revokeObjectURL(objUrl); } catch { /* ignored */ } }, 60000);
            return objUrl;
        } catch {
            return null;
        }
    }

    // ---- Person photo variants ---------------------------------------------------
    // Direct-browser-to-API upload mirroring the Book/MagazineIssue helpers. Person
    // photos POST to /people/{id}/photo/pending and read/delete via
    // /people/photo/pending/{guid}. Kept as separate functions for grep-friendliness
    // and so the existing Book/MagazineIssue wiring isn't perturbed.

    async function uploadPersonPhoto(apiBaseUrl, jwtToken, personId, fileInputElementOrId) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { ok: false, status: 0, error: 'No file selected.' };
        }

        const file = input.files[0];

        if (file.size > 50 * 1024 * 1024) {
            return { ok: false, status: 0, error: 'File exceeds 50 MB limit.' };
        }
        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
            return { ok: false, status: 0, error: 'Unsupported file type. Allowed: JPG, PNG, WebP.' };
        }

        const fd = new FormData();
        fd.append('file', file, file.name);

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/people/${encodeURIComponent(personId)}/photo/pending`;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${jwtToken}` },
                body: fd
            });

            if (!resp.ok) {
                let detail = '';
                try {
                    const body = await resp.text();
                    try {
                        const j = JSON.parse(body);
                        detail = j.detail || j.title || body;
                    } catch { detail = body; }
                } catch { /* ignored */ }
                return { ok: false, status: resp.status, error: detail || `HTTP ${resp.status}` };
            }

            const json = await resp.json();
            return { ok: true, status: resp.status, guid: json.guid, extension: json.extension };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    /**
     * Upload a pending photo for a brand-new person that doesn't exist yet — paired
     * with the addition-mode POST /suggestions flow (entityType=Person, entityId=null).
     * Mirrors uploadPersonPhoto without the personId path segment. Server stores the
     * sidecar with EntityId=0 and enforces the one-pending-per-uploader rule on that
     * surrogate id.
     */
    async function uploadNewPersonPhoto(apiBaseUrl, jwtToken, fileInputElementOrId) {
        const input = (typeof fileInputElementOrId === 'string')
            ? document.getElementById(fileInputElementOrId)
            : fileInputElementOrId;

        if (!input || !input.files || input.files.length === 0) {
            return { ok: false, status: 0, error: 'No file selected.' };
        }

        const file = input.files[0];

        if (file.size > 50 * 1024 * 1024) {
            return { ok: false, status: 0, error: 'File exceeds 50 MB limit.' };
        }
        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (file.type && !allowedTypes.includes(file.type.toLowerCase())) {
            return { ok: false, status: 0, error: 'Unsupported file type. Allowed: JPG, PNG, WebP.' };
        }

        const fd = new FormData();
        fd.append('file', file, file.name);

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/people/photo/pending/new`;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${jwtToken}` },
                body: fd
            });

            if (!resp.ok) {
                let detail = '';
                try {
                    const body = await resp.text();
                    try {
                        const j = JSON.parse(body);
                        detail = j.detail || j.title || body;
                    } catch { detail = body; }
                } catch { /* ignored */ }
                return { ok: false, status: resp.status, error: detail || `HTTP ${resp.status}` };
            }

            const json = await resp.json();
            return { ok: true, status: resp.status, guid: json.guid, extension: json.extension };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    async function deletePendingPersonPhoto(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/people/photo/pending/${encodeURIComponent(guid)}`;
        try {
            const resp = await fetch(url, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            return { ok: resp.ok, status: resp.status };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    async function applyPendingPersonPhotoImage(apiBaseUrl, jwtToken, guid, imgElementOrId) {
        const img = (typeof imgElementOrId === 'string')
            ? document.getElementById(imgElementOrId)
            : imgElementOrId;
        if (!img) return { ok: false, status: 0, error: 'Image element not found.' };

        if (img.dataset && img.dataset.pendingCoverUrl) {
            try { URL.revokeObjectURL(img.dataset.pendingCoverUrl); } catch { /* ignored */ }
            delete img.dataset.pendingCoverUrl;
        }

        const url = `${apiBaseUrl.replace(/\/+$/, '')}/people/photo/pending/${encodeURIComponent(guid)}`;

        try {
            const resp = await fetch(url, {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            if (!resp.ok) {
                return { ok: false, status: resp.status, error: `HTTP ${resp.status}` };
            }
            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            img.src = objUrl;
            if (img.dataset) img.dataset.pendingCoverUrl = objUrl;
            return { ok: true, status: resp.status };
        } catch (err) {
            return { ok: false, status: 0, error: err && err.message ? err.message : String(err) };
        }
    }

    async function openPendingPersonPhotoInNewTab(apiBaseUrl, jwtToken, guid) {
        const url = `${apiBaseUrl.replace(/\/+$/, '')}/people/photo/pending/${encodeURIComponent(guid)}`;
        try {
            const resp = await fetch(url, {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            if (!resp.ok) return null;
            const blob = await resp.blob();
            const objUrl = URL.createObjectURL(blob);
            window.open(objUrl, '_blank', 'noopener');
            setTimeout(() => { try { URL.revokeObjectURL(objUrl); } catch { /* ignored */ } }, 60000);
            return objUrl;
        } catch {
            return null;
        }
    }

    return {
        uploadBookCover,
        uploadNewBookCover,
        deletePendingCover,
        applyPendingCoverImage,
        openPendingCoverInNewTab,
        uploadMagazineIssueCover,
        uploadNewMagazineIssueCover,
        deletePendingMagazineIssueCover,
        applyPendingMagazineIssueCoverImage,
        openPendingMagazineIssueCoverInNewTab,
        uploadPersonPhoto,
        uploadNewPersonPhoto,
        deletePendingPersonPhoto,
        applyPendingPersonPhotoImage,
        openPendingPersonPhotoInNewTab,
        clickInput
    };
})();
