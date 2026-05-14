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

public partial class SoftwareScreenshotsSuggestionDialog : ComponentBase, IAsyncDisposable
{
    /// <summary>Hard ceiling on the number of images in a single batch (mirrors server enforcement).</summary>
    public const int MaxPhotos = 50;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public ulong  SoftwareId   { get; set; }
    [Parameter] public string SoftwareName { get; set; } = "";

    bool   _loading = true;
    bool   _submitting;
    string _validationError;
    string _userComment;

    /// <summary>Mandatory batch-level platform — applied uniformly to every accepted screenshot.</summary>
    ulong? _selectedPlatformId;

    List<SoftwarePlatformDto> _allPlatforms = new();

    // Staged images. Keyed by ClientGuid (a UUID generated in JS at upload-start time)
    // so the JS-side XHR progress callbacks can target the correct entry.
    readonly List<StagedScreenshot> _staged = new();

    // Stable id for the hidden file input (so JS clickInput can find it).
    readonly string _fileInputId = $"software-screenshot-input-{Guid.NewGuid():N}";

    DotNetObjectReference<SoftwareScreenshotsSuggestionDialog> _dotNetRef;

    protected override void OnInitialized()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _allPlatforms = await PlatformsService.GetAllAsync() ?? new List<SoftwarePlatformDto>();
            _allPlatforms = _allPlatforms.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch
        {
            _allPlatforms = new List<SoftwarePlatformDto>();
        }
        finally
        {
            _loading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // After every render, repaint thumbnails for any newly-ready staged image whose
        // <img> wasn't yet hydrated. This is idempotent — applyPendingPhotoImage revokes
        // the previous blob URL on each call.
        foreach(StagedScreenshot s in _staged)
        {
            if(s.Status != StagedScreenshotStatus.Ready) continue;
            if(s.ThumbnailHydrated) continue;
            try
            {
                string token = await TokenProvider.GetTokenAsync();
                if(string.IsNullOrEmpty(token)) continue;

                await JS.InvokeAsync<object>("MarechaiSoftwareScreenshotUpload.applyPendingPhotoImage",
                                             ApiAssetUrls.BaseUrl, token, s.ServerGuid.ToString("D"), s.ImgElementId);
                s.ThumbnailHydrated = true;
            }
            catch
            {
                // best-effort — preview is not critical
            }
        }
    }

    bool CanSubmit()
    {
        if(_loading) return false;
        if(!_selectedPlatformId.HasValue || _selectedPlatformId.Value == 0) return false;
        if(_staged.Count == 0) return false;
        if(_staged.Any(p => p.Status == StagedScreenshotStatus.Uploading)) return false;
        if(!_staged.All(p => p.Status == StagedScreenshotStatus.Ready)) return false;
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

            await JS.InvokeAsync<object>("MarechaiSoftwareScreenshotUpload.uploadSoftwareScreenshots",
                                         ApiAssetUrls.BaseUrl, token, SoftwareId, _fileInputId, _dotNetRef);
        }
        catch(Exception ex)
        {
            _validationError = ex.Message;
        }
    }

    async Task RemoveStagedAsync(string clientGuid)
    {
        StagedScreenshot s = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(s is null) return;

        // Cancel an in-flight XHR if the user removes mid-upload.
        if(s.Status == StagedScreenshotStatus.Uploading)
        {
            try { await JS.InvokeVoidAsync("MarechaiSoftwareScreenshotUpload.cancelUpload", s.ClientGuid); }
            catch { /* ignored */ }
        }

        // Delete the server-side pending file (no-op if upload never completed).
        if(s.ServerGuid != Guid.Empty)
        {
            await SoftwareService.DeletePendingScreenshotAsync(s.ServerGuid);
        }

        _staged.Remove(s);
        StateHasChanged();
    }

    // ───────────────────────────── JS-invokable callbacks ─────────────────────────────

    [JSInvokable]
    public Task OnPhotoUploadStarted(string clientGuid, string fileName, long sizeBytes)
    {
        _staged.Add(new StagedScreenshot
        {
            ClientGuid    = clientGuid,
            FileName      = fileName,
            SizeBytes     = sizeBytes,
            Status        = StagedScreenshotStatus.Uploading,
            UploadPercent = 0,
            ImgElementId  = $"software-screenshot-thumb-{Guid.NewGuid():N}"
        });
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadProgress(string clientGuid, double percent)
    {
        StagedScreenshot s = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(s is null) return Task.CompletedTask;
        s.UploadPercent = percent;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadCompleted(string clientGuid, string serverGuid, string extension, long sizeBytes)
    {
        StagedScreenshot s = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(s is null)
        {
            // Shouldn't happen — but if the C# side never saw OnPhotoUploadStarted (e.g.
            // a dropped SignalR message), synthesise an entry so the file isn't orphaned.
            s = new StagedScreenshot
            {
                ClientGuid   = clientGuid,
                FileName     = serverGuid,
                SizeBytes    = sizeBytes,
                ImgElementId = $"software-screenshot-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(s);
        }

        s.Status        = StagedScreenshotStatus.Ready;
        s.UploadPercent = 100;
        s.SizeBytes     = sizeBytes;
        s.Extension     = extension;
        if(Guid.TryParse(serverGuid, out Guid g)) s.ServerGuid = g;

        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadFailed(string clientGuid, string error)
    {
        StagedScreenshot s = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(s is null)
        {
            // Synthesise an error entry so the user sees the failure.
            s = new StagedScreenshot
            {
                ClientGuid   = clientGuid,
                FileName     = error,
                ImgElementId = $"software-screenshot-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(s);
        }

        s.Status    = StagedScreenshotStatus.Error;
        s.ErrorText = error;
        StateHasChanged();
        return Task.CompletedTask;
    }

    // ───────────────────────────── Submit / Cancel ─────────────────────────────

    async Task SubmitAsync()
    {
        _validationError = null;

        if(!CanSubmit())
        {
            if(!_selectedPlatformId.HasValue || _selectedPlatformId.Value == 0)
                _validationError = L["Pick a platform first."].Value;
            else
                _validationError =
                    L["Add at least one successfully-uploaded image before submitting."].Value;
            return;
        }

        var photos = _staged
                     .Where(p => p.Status == StagedScreenshotStatus.Ready)
                     .Select(p => new
                     {
                         guid      = p.ServerGuid.ToString("D", CultureInfo.InvariantCulture),
                         extension = p.Extension,
                         caption   = p.Caption ?? string.Empty
                     })
                     .ToArray();

        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["platform_id"] = _selectedPlatformId!.Value,
            ["photos"]      = photos
        };

        _submitting = true;
        try
        {
            string json = JsonSerializer.Serialize(values);
            var dto = new SuggestionDto
            {
                EntityType          = (int?)SuggestionEntityType.SoftwareScreenshot,
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

            Snackbar.Add(L["Screenshots submitted for review. An administrator will accept or reject each image."],
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
        foreach(StagedScreenshot s in _staged.ToList())
        {
            if(s.Status == StagedScreenshotStatus.Uploading)
            {
                try { await JS.InvokeVoidAsync("MarechaiSoftwareScreenshotUpload.cancelUpload", s.ClientGuid); }
                catch { /* ignored */ }
            }

            if(s.ServerGuid != Guid.Empty)
            {
                try { await SoftwareService.DeletePendingScreenshotAsync(s.ServerGuid); } catch { /* ignored */ }
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

    sealed class StagedScreenshot
    {
        public string                 ClientGuid        { get; init; }
        public string                 FileName          { get; set; }
        public long                   SizeBytes         { get; set; }
        public string                 Extension         { get; set; }
        public Guid                   ServerGuid        { get; set; }
        public StagedScreenshotStatus Status            { get; set; }
        public double                 UploadPercent     { get; set; }
        public string                 ErrorText         { get; set; }
        public string                 Caption           { get; set; }
        public string                 ImgElementId      { get; init; }
        public bool                   ThumbnailHydrated { get; set; }
    }

    enum StagedScreenshotStatus
    {
        Uploading,
        Ready,
        Error
    }
}
