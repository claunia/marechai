/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Marechai.Pages.Suggestions;

public partial class ProcessorPhotosSuggestionDialog : ComponentBase, IAsyncDisposable
{
    /// <summary>Hard ceiling on the number of photos in a single batch (mirrors server enforcement).</summary>
    public const int MaxPhotos = 15;

    /// <summary>SPDX ids treated as Public Domain in addition to the CC- prefix family. Mirrors the server allowlist.</summary>
    static readonly HashSet<string> s_publicDomainSpdxIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "CC0-1.0", "Unlicense"
    };

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public int    ProcessorId   { get; set; }
    [Parameter] public string ProcessorName { get; set; } = "";

    bool   _loading = true;
    bool   _submitting;
    string _validationError;
    string _userComment;

    // Suggestion-level batch metadata
    int?   _licenseId;
    string _sourceUrl;

    // Filtered license list (CC + curated PD, computed once at OnInitializedAsync time)
    List<LicenseDto> _allowedLicenses = new();

    // Staged photos. Keyed by ClientGuid (a UUID generated in JS at upload-start time)
    // so the JS-side XHR progress callbacks can target the correct entry.
    readonly List<StagedPhoto> _staged = new();

    // Stable ids for the hidden file input (so JS clickInput can find it).
    readonly string _fileInputId = $"processor-photo-input-{Guid.NewGuid():N}";

    DotNetObjectReference<ProcessorPhotosSuggestionDialog> _dotNetRef;

    protected override async Task OnInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);

        try
        {
            List<LicenseDto> all = await ProcessorPhotosService.GetAllLicensesAsync();
            _allowedLicenses = all.Where(IsCreativeCommonsOrPublicDomain).ToList();
        }
        finally
        {
            _loading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // After every render, repaint thumbnails for any newly-ready staged photo whose
        // <img> wasn't yet hydrated. This is idempotent — applyPendingPhotoImage revokes
        // the previous blob URL on each call.
        foreach(StagedPhoto p in _staged)
        {
            if(p.Status != StagedPhotoStatus.Ready) continue;
            if(p.ThumbnailHydrated) continue;
            try
            {
                string token = await TokenProvider.GetTokenAsync();
                if(string.IsNullOrEmpty(token)) continue;

                await JS.InvokeAsync<object>("MarechaiProcessorPhotoUpload.applyPendingPhotoImage",
                                              ApiAssetUrls.BaseUrl, token, p.ServerGuid.ToString("D"), p.ImgElementId);
                p.ThumbnailHydrated = true;
            }
            catch
            {
                // best-effort — preview is not critical
            }
        }
    }

    bool CanSubmit()
    {
        if(!_licenseId.HasValue) return false;
        if(_staged.Count == 0) return false;
        if(_staged.Any(p => p.Status == StagedPhotoStatus.Uploading)) return false;
        if(!_staged.Any(p => p.Status == StagedPhotoStatus.Ready)) return false;
        return true;
    }

    async Task TriggerFilePicker()
    {
        try
        {
            await JS.InvokeVoidAsync("MarechaiPendingCover.clickInput", _fileInputId);
        }
        catch
        {
            // best-effort
        }
    }

    async Task OnFilesPicked()
    {
        try
        {
            string token = await TokenProvider.GetTokenAsync();
            if(string.IsNullOrEmpty(token))
            {
                _validationError = L["You must be logged in to upload photos."].Value;
                return;
            }

            await JS.InvokeAsync<object>("MarechaiProcessorPhotoUpload.uploadProcessorPhotos",
                                          ApiAssetUrls.BaseUrl, token, ProcessorId, _fileInputId, _dotNetRef);
        }
        catch(Exception ex)
        {
            _validationError = ex.Message;
        }
    }

    async Task RemoveStagedPhotoAsync(string clientGuid)
    {
        StagedPhoto p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null) return;

        // Cancel an in-flight XHR if the user removes mid-upload.
        if(p.Status == StagedPhotoStatus.Uploading)
        {
            try { await JS.InvokeVoidAsync("MarechaiProcessorPhotoUpload.cancelUpload", p.ClientGuid); } catch { /* ignored */ }
        }

        // Delete the server-side pending file (no-op if upload never completed).
        if(p.ServerGuid != Guid.Empty)
        {
            await ProcessorPhotosService.DeletePendingPhotoAsync(p.ServerGuid);
        }

        _staged.Remove(p);
        StateHasChanged();
    }

    // ───────────────────────────── JS-invokable callbacks ─────────────────────────────

    [JSInvokable]
    public Task OnPhotoUploadStarted(string clientGuid, string fileName, long sizeBytes)
    {
        _staged.Add(new StagedPhoto
        {
            ClientGuid    = clientGuid,
            FileName      = fileName,
            SizeBytes     = sizeBytes,
            Status        = StagedPhotoStatus.Uploading,
            UploadPercent = 0,
            ImgElementId  = $"processor-photo-thumb-{Guid.NewGuid():N}"
        });
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadProgress(string clientGuid, double percent)
    {
        StagedPhoto p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null) return Task.CompletedTask;
        p.UploadPercent = percent;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadCompleted(string clientGuid, string serverGuid, string extension, long sizeBytes)
    {
        StagedPhoto p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null)
        {
            // Shouldn't happen — but if the C# side never saw OnPhotoUploadStarted (e.g.
            // a dropped SignalR message), synthesise an entry so the file isn't orphaned.
            p = new StagedPhoto
            {
                ClientGuid   = clientGuid,
                FileName     = serverGuid,
                SizeBytes    = sizeBytes,
                ImgElementId = $"processor-photo-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(p);
        }

        p.Status        = StagedPhotoStatus.Ready;
        p.UploadPercent = 100;
        p.SizeBytes     = sizeBytes;
        p.Extension     = extension;
        if(Guid.TryParse(serverGuid, out Guid g)) p.ServerGuid = g;

        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadFailed(string clientGuid, string error)
    {
        StagedPhoto p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null)
        {
            // Synthesise an error entry so the user sees the failure.
            p = new StagedPhoto
            {
                ClientGuid   = clientGuid,
                FileName     = error,
                ImgElementId = $"processor-photo-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(p);
        }

        p.Status    = StagedPhotoStatus.Error;
        p.ErrorText = error;
        StateHasChanged();
        return Task.CompletedTask;
    }

    // ───────────────────────────── Submit / Cancel ─────────────────────────────

    async Task SubmitAsync()
    {
        _validationError = null;

        if(!CanSubmit())
        {
            _validationError = L["Pick a license and at least one successfully-uploaded photo before submitting."].Value;
            return;
        }

        // Defence in depth: server re-checks the license.
        LicenseDto license = _allowedLicenses.FirstOrDefault(l => l.Id == _licenseId);
        if(license is null)
        {
            _validationError = L["Selected license is not allowed for collaborator uploads."].Value;
            return;
        }

        var photos = _staged
                     .Where(p => p.Status == StagedPhotoStatus.Ready)
                     .Select(p => new
                     {
                         guid      = p.ServerGuid.ToString("D", CultureInfo.InvariantCulture),
                         extension = p.Extension,
                         comment   = p.Comment ?? string.Empty
                     })
                     .ToArray();

        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["license_id"] = _licenseId.Value,
            ["source_url"] = _sourceUrl ?? string.Empty,
            ["photos"]     = photos
        };

        _submitting = true;
        try
        {
            string json = JsonSerializer.Serialize(values);
            var dto = new SuggestionDto
            {
                EntityType          = (int?)SuggestionEntityType.ProcessorPhoto,
                EntityId            = ProcessorId,
                Status              = (int?)SuggestionStatus.Pending,
                UserComment         = string.IsNullOrWhiteSpace(_userComment) ? null : _userComment.Trim(),
                SuggestedValuesJson = json
            };

            var (created, error) = await SuggestionsService.CreateAsync(dto);

            if(created is null)
            {
                _validationError = string.IsNullOrWhiteSpace(error) ? L["Failed to submit suggestion."].Value : error;
                return;
            }

            // Photos are now owned by the suggestion row — don't run the cancel cleanup.
            _staged.Clear();

            Snackbar.Add(L["Photos submitted for review. An administrator will accept or reject each one."],
                         Severity.Success);
            MudDialog.Close(DialogResult.Ok(created));
        }
        finally
        {
            _submitting = false;
        }
    }

    async Task OnCancelAsync()
    {
        if(_staged.Count == 0)
        {
            MudDialog.Cancel();
            return;
        }

        bool confirmed;
        try
        {
            confirmed = await JS.InvokeAsync<bool>("confirm",
                            L["You have staged photos that have not been submitted yet. Closing this dialog will delete them. Continue?"].Value);
        }
        catch
        {
            // If JS is unavailable, default to keeping the user safe (don't discard).
            confirmed = false;
        }

        if(!confirmed) return;

        // Best-effort cleanup: cancel in-flight XHRs, then DELETE every uploaded pending file.
        foreach(StagedPhoto p in _staged.ToList())
        {
            if(p.Status == StagedPhotoStatus.Uploading)
            {
                try { await JS.InvokeVoidAsync("MarechaiProcessorPhotoUpload.cancelUpload", p.ClientGuid); } catch { /* ignored */ }
            }

            if(p.ServerGuid != Guid.Empty)
            {
                try { await ProcessorPhotosService.DeletePendingPhotoAsync(p.ServerGuid); } catch { /* ignored */ }
            }
        }

        _staged.Clear();
        MudDialog.Cancel();
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        await Task.CompletedTask;
    }

    // ───────────────────────────── helpers ─────────────────────────────

    static bool IsCreativeCommonsOrPublicDomain(LicenseDto l)
    {
        if(l is null) return false;

        string spdx = l.Spdx;

        if(!string.IsNullOrEmpty(spdx))
        {
            if(spdx.StartsWith("CC-", StringComparison.OrdinalIgnoreCase)) return true;
            if(s_publicDomainSpdxIds.Contains(spdx)) return true;
        }

        return string.Equals(l.Name, "Public Domain", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(l.Name, "Public domain", StringComparison.OrdinalIgnoreCase);
    }

    static string FormatLicense(LicenseDto l)
    {
        if(l is null) return string.Empty;
        return string.IsNullOrEmpty(l.Spdx) ? l.Name : $"{l.Name} ({l.Spdx})";
    }

    static string FormatBytes(long bytes)
    {
        if(bytes < 1024) return $"{bytes} B";
        if(bytes < 1024 * 1024) return $"{bytes / 1024d:0.0} KB";
        return $"{bytes / (1024d * 1024d):0.0} MB";
    }

    sealed class StagedPhoto
    {
        public string                ClientGuid       { get; init; }
        public string                FileName         { get; set; }
        public long                  SizeBytes        { get; set; }
        public string                Extension        { get; set; }
        public Guid                  ServerGuid       { get; set; }
        public StagedPhotoStatus     Status           { get; set; }
        public double                UploadPercent    { get; set; }
        public string                ErrorText        { get; set; }
        public string                Comment          { get; set; }
        public string                ImgElementId     { get; init; }
        public bool                  ThumbnailHydrated { get; set; }
    }

    enum StagedPhotoStatus
    {
        Uploading,
        Ready,
        Error
    }
}
