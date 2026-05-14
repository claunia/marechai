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

public partial class SoftwareCoversSuggestionDialog : ComponentBase, IAsyncDisposable
{
    /// <summary>Hard ceiling on the number of images in a single batch (mirrors server enforcement).</summary>
    public const int MaxPhotos = 30;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public ulong  SoftwareReleaseId { get; set; }
    [Parameter] public string ReleaseTitle      { get; set; } = "";

    bool   _submitting;
    string _validationError;
    string _userComment;

    // Staged images. Keyed by ClientGuid (a UUID generated in JS at upload-start time)
    // so the JS-side XHR progress callbacks can target the correct entry.
    readonly List<StagedCover> _staged = new();

    // Stable id for the hidden file input (so JS clickInput can find it).
    readonly string _fileInputId = $"software-cover-input-{Guid.NewGuid():N}";

    DotNetObjectReference<SoftwareCoversSuggestionDialog> _dotNetRef;

    protected override void OnInitialized()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // After every render, repaint thumbnails for any newly-ready staged image whose
        // <img> wasn't yet hydrated. This is idempotent — applyPendingPhotoImage revokes
        // the previous blob URL on each call.
        foreach(StagedCover c in _staged)
        {
            if(c.Status != StagedCoverStatus.Ready) continue;
            if(c.ThumbnailHydrated) continue;
            try
            {
                string token = await TokenProvider.GetTokenAsync();
                if(string.IsNullOrEmpty(token)) continue;

                await JS.InvokeAsync<object>("MarechaiSoftwareCoverUpload.applyPendingPhotoImage",
                                              ApiAssetUrls.BaseUrl, token, c.ServerGuid.ToString("D"), c.ImgElementId);
                c.ThumbnailHydrated = true;
            }
            catch
            {
                // best-effort — preview is not critical
            }
        }
    }

    bool CanSubmit()
    {
        if(_staged.Count == 0) return false;
        if(_staged.Any(p => p.Status == StagedCoverStatus.Uploading)) return false;
        if(!_staged.All(p => p.Status == StagedCoverStatus.Ready && p.Type.HasValue)) return false;
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

            await JS.InvokeAsync<object>("MarechaiSoftwareCoverUpload.uploadSoftwareCovers",
                                          ApiAssetUrls.BaseUrl, token, SoftwareReleaseId, _fileInputId, _dotNetRef);
        }
        catch(Exception ex)
        {
            _validationError = ex.Message;
        }
    }

    async Task RemoveStagedAsync(string clientGuid)
    {
        StagedCover c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null) return;

        // Cancel an in-flight XHR if the user removes mid-upload.
        if(c.Status == StagedCoverStatus.Uploading)
        {
            try { await JS.InvokeVoidAsync("MarechaiSoftwareCoverUpload.cancelUpload", c.ClientGuid); }
            catch { /* ignored */ }
        }

        // Delete the server-side pending file (no-op if upload never completed).
        if(c.ServerGuid != Guid.Empty)
        {
            await ReleasesService.DeletePendingCoverAsync(c.ServerGuid);
        }

        _staged.Remove(c);
        StateHasChanged();
    }

    // ───────────────────────────── JS-invokable callbacks ─────────────────────────────

    [JSInvokable]
    public Task OnPhotoUploadStarted(string clientGuid, string fileName, long sizeBytes)
    {
        _staged.Add(new StagedCover
        {
            ClientGuid    = clientGuid,
            FileName      = fileName,
            SizeBytes     = sizeBytes,
            Status        = StagedCoverStatus.Uploading,
            UploadPercent = 0,
            ImgElementId  = $"software-cover-thumb-{Guid.NewGuid():N}"
        });
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadProgress(string clientGuid, double percent)
    {
        StagedCover c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null) return Task.CompletedTask;
        c.UploadPercent = percent;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadCompleted(string clientGuid, string serverGuid, string extension, long sizeBytes)
    {
        StagedCover c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null)
        {
            // Shouldn't happen — but if the C# side never saw OnPhotoUploadStarted (e.g.
            // a dropped SignalR message), synthesise an entry so the file isn't orphaned.
            c = new StagedCover
            {
                ClientGuid   = clientGuid,
                FileName     = serverGuid,
                SizeBytes    = sizeBytes,
                ImgElementId = $"software-cover-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(c);
        }

        c.Status        = StagedCoverStatus.Ready;
        c.UploadPercent = 100;
        c.SizeBytes     = sizeBytes;
        c.Extension     = extension;
        if(Guid.TryParse(serverGuid, out Guid g)) c.ServerGuid = g;

        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadFailed(string clientGuid, string error)
    {
        StagedCover c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null)
        {
            // Synthesise an error entry so the user sees the failure.
            c = new StagedCover
            {
                ClientGuid   = clientGuid,
                FileName     = error,
                ImgElementId = $"software-cover-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(c);
        }

        c.Status    = StagedCoverStatus.Error;
        c.ErrorText = error;
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
                L["Add at least one successfully-uploaded image and pick a type for each before submitting."].Value;
            return;
        }

        var photos = _staged
                     .Where(p => p.Status == StagedCoverStatus.Ready && p.Type.HasValue)
                     .Select(p => new
                     {
                         guid      = p.ServerGuid.ToString("D", CultureInfo.InvariantCulture),
                         extension = p.Extension,
                         type      = (int)p.Type!.Value,
                         caption   = p.Caption ?? string.Empty
                     })
                     .ToArray();

        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["photos"] = photos
        };

        _submitting = true;
        try
        {
            string json = JsonSerializer.Serialize(values);
            var dto = new SuggestionDto
            {
                EntityType          = (int?)SuggestionEntityType.SoftwareCover,
                EntityId            = (long)SoftwareReleaseId,
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

            Snackbar.Add(L["Covers submitted for review. An administrator will accept or reject each image."],
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
        foreach(StagedCover c in _staged.ToList())
        {
            if(c.Status == StagedCoverStatus.Uploading)
            {
                try { await JS.InvokeVoidAsync("MarechaiSoftwareCoverUpload.cancelUpload", c.ClientGuid); }
                catch { /* ignored */ }
            }

            if(c.ServerGuid != Guid.Empty)
            {
                try { await ReleasesService.DeletePendingCoverAsync(c.ServerGuid); } catch { /* ignored */ }
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

    sealed class StagedCover
    {
        public string             ClientGuid        { get; init; }
        public string             FileName          { get; set; }
        public long               SizeBytes         { get; set; }
        public string             Extension         { get; set; }
        public Guid               ServerGuid        { get; set; }
        public StagedCoverStatus  Status            { get; set; }
        public double             UploadPercent     { get; set; }
        public string             ErrorText         { get; set; }
        public SoftwareCoverType? Type              { get; set; }
        public string             Caption           { get; set; }
        public string             ImgElementId      { get; init; }
        public bool               ThumbnailHydrated { get; set; }
    }

    enum StagedCoverStatus
    {
        Uploading,
        Ready,
        Error
    }
}
