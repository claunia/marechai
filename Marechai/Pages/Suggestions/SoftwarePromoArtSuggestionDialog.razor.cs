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
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Marechai.Pages.Suggestions;

public partial class SoftwarePromoArtSuggestionDialog : ComponentBase, IAsyncDisposable
{
    /// <summary>Hard ceiling on the number of images in a single batch (mirrors server enforcement).</summary>
    public const int MaxPhotos = 30;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public ulong  SoftwareId   { get; set; }
    [Parameter] public string SoftwareName { get; set; } = "";

    bool   _loading = true;
    bool   _submitting;
    string _validationError;
    string _userComment;

    // Suggestion-level batch metadata: free-text group name with autocomplete over
    // existing groups. Server does get-or-create on accept.
    string       _groupName;
    List<string> _existingGroups = new();

    // Map of localized display name -> canonical English name. Populated alongside
    // _existingGroups so submit can resolve the displayed value back to canonical and
    // avoid creating duplicate groups when the UI is in a non-English locale.
    Dictionary<string, string> _groupCanonicalByDisplayName = new(StringComparer.OrdinalIgnoreCase);

    // Staged images. Keyed by ClientGuid (a UUID generated in JS at upload-start time)
    // so the JS-side XHR progress callbacks can target the correct entry.
    readonly List<StagedPromo> _staged = new();

    // Stable id for the hidden file input (so JS clickInput can find it).
    readonly string _fileInputId = $"software-promo-art-input-{Guid.NewGuid():N}";

    DotNetObjectReference<SoftwarePromoArtSuggestionDialog> _dotNetRef;

    protected override async Task OnInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);

        try
        {
            List<SoftwarePromoArtGroupDto> groups = await SoftwareService.GetPromoArtGroupsAsync();
            _existingGroups = groups
                              .Where(g => !string.IsNullOrWhiteSpace(g.Name))
                              .Select(g => g.Name)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                              .ToList();

            _groupCanonicalByDisplayName = groups
                                          .Where(g => !string.IsNullOrWhiteSpace(g.Name) &&
                                                      !string.IsNullOrWhiteSpace(g.CanonicalName))
                                          .GroupBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                                          .ToDictionary(g => g.Key, g => g.First().CanonicalName,
                                                        StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            _existingGroups              = new List<string>();
            _groupCanonicalByDisplayName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            _loading = false;
        }
    }

    Task<IEnumerable<string>> SearchGroupsAsync(string value, CancellationToken ct)
    {
        IEnumerable<string> results = string.IsNullOrWhiteSpace(value)
                                          ? _existingGroups.Take(25)
                                          : _existingGroups
                                            .Where(g => g.Contains(value, StringComparison.OrdinalIgnoreCase))
                                            .Take(25);

        return Task.FromResult(results);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // After every render, repaint thumbnails for any newly-ready staged image whose
        // <img> wasn't yet hydrated. This is idempotent — applyPendingPhotoImage revokes
        // the previous blob URL on each call.
        foreach(StagedPromo p in _staged)
        {
            if(p.Status != StagedPromoStatus.Ready) continue;
            if(p.ThumbnailHydrated) continue;
            try
            {
                string token = await TokenProvider.GetTokenAsync();
                if(string.IsNullOrEmpty(token)) continue;

                await JS.InvokeAsync<object>("MarechaiSoftwarePromoArtUpload.applyPendingPhotoImage",
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
        if(string.IsNullOrWhiteSpace(_groupName)) return false;
        if(_groupName.Trim().Length > 256) return false;
        if(_staged.Count == 0) return false;
        if(_staged.Any(p => p.Status == StagedPromoStatus.Uploading)) return false;
        if(!_staged.Any(p => p.Status == StagedPromoStatus.Ready)) return false;
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
                _validationError = L["You must be logged in to upload images."].Value;
                return;
            }

            await JS.InvokeAsync<object>("MarechaiSoftwarePromoArtUpload.uploadSoftwarePromoArtImages",
                                          ApiAssetUrls.BaseUrl, token, SoftwareId, _fileInputId, _dotNetRef);
        }
        catch(Exception ex)
        {
            _validationError = ex.Message;
        }
    }

    async Task RemoveStagedAsync(string clientGuid)
    {
        StagedPromo p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null) return;

        // Cancel an in-flight XHR if the user removes mid-upload.
        if(p.Status == StagedPromoStatus.Uploading)
        {
            try { await JS.InvokeVoidAsync("MarechaiSoftwarePromoArtUpload.cancelUpload", p.ClientGuid); }
            catch { /* ignored */ }
        }

        // Delete the server-side pending file (no-op if upload never completed).
        if(p.ServerGuid != Guid.Empty)
        {
            await SoftwareService.DeletePendingPromoArtAsync(p.ServerGuid);
        }

        _staged.Remove(p);
        StateHasChanged();
    }

    // ───────────────────────────── JS-invokable callbacks ─────────────────────────────

    [JSInvokable]
    public Task OnPhotoUploadStarted(string clientGuid, string fileName, long sizeBytes)
    {
        _staged.Add(new StagedPromo
        {
            ClientGuid    = clientGuid,
            FileName      = fileName,
            SizeBytes     = sizeBytes,
            Status        = StagedPromoStatus.Uploading,
            UploadPercent = 0,
            ImgElementId  = $"software-promo-art-thumb-{Guid.NewGuid():N}"
        });
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadProgress(string clientGuid, double percent)
    {
        StagedPromo p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null) return Task.CompletedTask;
        p.UploadPercent = percent;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadCompleted(string clientGuid, string serverGuid, string extension, long sizeBytes)
    {
        StagedPromo p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null)
        {
            // Shouldn't happen — but if the C# side never saw OnPhotoUploadStarted (e.g.
            // a dropped SignalR message), synthesise an entry so the file isn't orphaned.
            p = new StagedPromo
            {
                ClientGuid   = clientGuid,
                FileName     = serverGuid,
                SizeBytes    = sizeBytes,
                ImgElementId = $"software-promo-art-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(p);
        }

        p.Status        = StagedPromoStatus.Ready;
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
        StagedPromo p = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(p is null)
        {
            // Synthesise an error entry so the user sees the failure.
            p = new StagedPromo
            {
                ClientGuid   = clientGuid,
                FileName     = error,
                ImgElementId = $"software-promo-art-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(p);
        }

        p.Status    = StagedPromoStatus.Error;
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
            _validationError =
                L["Type or pick a group name and add at least one successfully-uploaded image before submitting."].Value;
            return;
        }

        var photos = _staged
                     .Where(p => p.Status == StagedPromoStatus.Ready)
                     .Select(p => new
                     {
                         guid      = p.ServerGuid.ToString("D", CultureInfo.InvariantCulture),
                         extension = p.Extension,
                         caption   = p.Caption ?? string.Empty
                     })
                     .ToArray();

        // Resolve the displayed (potentially localized) group name back to its canonical English
        // form so the server-side get-or-create matches an existing group regardless of locale.
        // Free-text new entries fall through and get accepted as-is — the worker will translate
        // them on the next tick.
        string trimmedGroup = _groupName.Trim();
        string canonicalGroupName =
            _groupCanonicalByDisplayName.TryGetValue(trimmedGroup, out string canonical)
                ? canonical
                : trimmedGroup;

        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["group_name"] = canonicalGroupName,
            ["photos"]     = photos
        };

        _submitting = true;
        try
        {
            string json = JsonSerializer.Serialize(values);
            var dto = new SuggestionDto
            {
                EntityType          = (int?)SuggestionEntityType.SoftwarePromoArt,
                EntityId            = (long)SoftwareId,
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

            // Images are now owned by the suggestion row — don't run the cancel cleanup.
            _staged.Clear();

            Snackbar.Add(L["Promo art submitted for review. An administrator will accept or reject each image."],
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
                            L["You have staged images that have not been submitted yet. Closing this dialog will delete them. Continue?"].Value);
        }
        catch
        {
            // If JS is unavailable, default to keeping the user safe (don't discard).
            confirmed = false;
        }

        if(!confirmed) return;

        // Best-effort cleanup: cancel in-flight XHRs, then DELETE every uploaded pending file.
        foreach(StagedPromo p in _staged.ToList())
        {
            if(p.Status == StagedPromoStatus.Uploading)
            {
                try { await JS.InvokeVoidAsync("MarechaiSoftwarePromoArtUpload.cancelUpload", p.ClientGuid); }
                catch { /* ignored */ }
            }

            if(p.ServerGuid != Guid.Empty)
            {
                try { await SoftwareService.DeletePendingPromoArtAsync(p.ServerGuid); } catch { /* ignored */ }
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

    static string FormatBytes(long bytes)
    {
        if(bytes < 1024) return $"{bytes} B";
        if(bytes < 1024 * 1024) return $"{bytes / 1024d:0.0} KB";
        return $"{bytes / (1024d * 1024d):0.0} MB";
    }

    sealed class StagedPromo
    {
        public string             ClientGuid        { get; init; }
        public string             FileName          { get; set; }
        public long               SizeBytes         { get; set; }
        public string             Extension         { get; set; }
        public Guid               ServerGuid        { get; set; }
        public StagedPromoStatus  Status            { get; set; }
        public double             UploadPercent     { get; set; }
        public string             ErrorText         { get; set; }
        public string             Caption           { get; set; }
        public string             ImgElementId      { get; init; }
        public bool               ThumbnailHydrated { get; set; }
    }

    enum StagedPromoStatus
    {
        Uploading,
        Ready,
        Error
    }
}
