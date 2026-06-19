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

public partial class MachinePromoArtSuggestionDialog : ComponentBase, IAsyncDisposable
{
    public const int MaxPhotos = 30;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public int    MachineId   { get; set; }
    [Parameter] public string MachineName { get; set; } = "";

    bool   _loading = true;
    bool   _submitting;
    string _validationError;
    string _userComment;
    string _groupName;
    List<string> _existingGroups = [];
    Dictionary<string, string> _groupCanonicalByDisplayName = new(StringComparer.OrdinalIgnoreCase);
    readonly List<StagedPromo> _staged = [];
    readonly string _fileInputId = $"machine-promo-art-input-{Guid.NewGuid():N}";
    DotNetObjectReference<MachinePromoArtSuggestionDialog> _dotNetRef;

    protected override async Task OnInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);

        try
        {
            List<SoftwarePromoArtGroupDto> groups = await MachinePromoArtService.GetPromoArtGroupsAsync();
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
            _existingGroups = [];
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
        foreach(StagedPromo p in _staged)
        {
            if(p.Status != StagedPromoStatus.Ready) continue;
            if(p.ThumbnailHydrated) continue;

            try
            {
                string token = await TokenProvider.GetTokenAsync();
                if(string.IsNullOrEmpty(token)) continue;

                await JS.InvokeAsync<object>("MarechaiMachinePromoArtUpload.applyPendingPhotoImage",
                                             ApiAssetUrls.BaseUrl, token, p.ServerGuid.ToString("D"), p.ImgElementId);
                p.ThumbnailHydrated = true;
            }
            catch
            {
                // best-effort preview
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

            await JS.InvokeAsync<object>("MarechaiMachinePromoArtUpload.uploadMachinePromoArtImages",
                                         ApiAssetUrls.BaseUrl, token, MachineId, _fileInputId, _dotNetRef);
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

        if(p.Status == StagedPromoStatus.Uploading)
        {
            try { await JS.InvokeVoidAsync("MarechaiMachinePromoArtUpload.cancelUpload", p.ClientGuid); }
            catch { /* ignored */ }
        }

        if(p.ServerGuid != Guid.Empty)
            await MachinePromoArtService.DeletePendingPromoArtAsync(p.ServerGuid);

        _staged.Remove(p);
        StateHasChanged();
    }

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
            ImgElementId  = $"machine-promo-art-thumb-{Guid.NewGuid():N}"
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
            p = new StagedPromo
            {
                ClientGuid   = clientGuid,
                FileName     = serverGuid,
                SizeBytes    = sizeBytes,
                ImgElementId = $"machine-promo-art-thumb-{Guid.NewGuid():N}"
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
            p = new StagedPromo
            {
                ClientGuid   = clientGuid,
                FileName     = error,
                ImgElementId = $"machine-promo-art-thumb-{Guid.NewGuid():N}"
            };
            _staged.Add(p);
        }

        p.Status    = StagedPromoStatus.Error;
        p.ErrorText = error;
        StateHasChanged();
        return Task.CompletedTask;
    }

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
                EntityType          = (int?)SuggestionEntityType.MachinePromoArt,
                EntityId            = MachineId,
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
            confirmed = false;
        }

        if(!confirmed) return;

        foreach(StagedPromo p in _staged.ToList())
        {
            if(p.Status == StagedPromoStatus.Uploading)
            {
                try { await JS.InvokeVoidAsync("MarechaiMachinePromoArtUpload.cancelUpload", p.ClientGuid); }
                catch { /* ignored */ }
            }

            if(p.ServerGuid != Guid.Empty)
            {
                try { await MachinePromoArtService.DeletePendingPromoArtAsync(p.ServerGuid); } catch { /* ignored */ }
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

    static string FormatBytes(long bytes)
    {
        if(bytes < 1024) return $"{bytes} B";
        if(bytes < 1024 * 1024) return $"{bytes / 1024d:0.0} KB";
        return $"{bytes / (1024d * 1024d):0.0} MB";
    }

    sealed class StagedPromo
    {
        public string            ClientGuid        { get; init; }
        public string            FileName          { get; set; }
        public long              SizeBytes         { get; set; }
        public string            Extension         { get; set; }
        public Guid              ServerGuid        { get; set; }
        public StagedPromoStatus Status            { get; set; }
        public double            UploadPercent     { get; set; }
        public string            ErrorText         { get; set; }
        public string            Caption           { get; set; }
        public string            ImgElementId      { get; init; }
        public bool              ThumbnailHydrated { get; set; }
    }

    enum StagedPromoStatus
    {
        Uploading,
        Ready,
        Error
    }
}
