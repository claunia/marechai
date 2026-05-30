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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoundSynthPhotosBatchUploadDialog : ComponentBase, IAsyncDisposable
{
    public const int MaxImages = 25;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public int                SoundSynthId   { get; set; }
    [Parameter] public string             SoundSynthName { get; set; } = "";
    [Parameter] public List<LicenseDto>   Licenses       { get; set; } = new();

    enum Phase { Staging, Committing, Done }

    Phase      _phase = Phase.Staging;
    string     _validationError;
    LicenseDto _selectedLicense;

    readonly List<StagedPhoto> _staged      = new();
    readonly string            _fileInputId = $"ss-batch-input-{Guid.NewGuid():N}";

    DotNetObjectReference<SoundSynthPhotosBatchUploadDialog> _dotNetRef;

    Guid?                                                _jobId;
    int                                                  _jobTotal;
    int                                                  _jobProcessed;
    Guid?                                                _currentPendingId;
    List<AdminSoundSynthPhotoBatchJobItemResultDto>      _jobResults = new();
    CancellationTokenSource                              _pollCts;

    double _jobPercent => _jobTotal <= 0 ? 0 : Math.Min(100, _jobProcessed * 100.0 / _jobTotal);

    protected override void OnInitialized()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    bool CanSubmit()
    {
        if(_selectedLicense is null) return false;
        if(_staged.Count == 0) return false;
        if(_staged.Any(s => s.Status == StagedPhotoStatus.Uploading)) return false;
        if(!_staged.All(s => s.Status == StagedPhotoStatus.Ready)) return false;
        return true;
    }

    async Task TriggerFilePicker()
    {
        try { await JS.InvokeVoidAsync("MarechaiSoundSynthPhotoBatchUpload.clickInput", _fileInputId); }
        catch { /* best-effort */ }
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

            int slotsLeft = Math.Max(0, MaxImages - _staged.Count);
            await JS.InvokeAsync<object>("MarechaiSoundSynthPhotoBatchUpload.uploadBatch",
                                         ApiAssetUrls.BaseUrl, token, SoundSynthId, _fileInputId,
                                         _dotNetRef, slotsLeft);
        }
        catch(Exception ex)
        {
            _validationError = ex.Message;
        }
    }

    async Task RemoveStagedAsync(string clientGuid)
    {
        StagedPhoto c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null) return;

        if(c.Status == StagedPhotoStatus.Uploading)
        {
            try { await JS.InvokeVoidAsync("MarechaiSoundSynthPhotoBatchUpload.cancelUpload", c.ClientGuid); }
            catch { /* ignored */ }
        }

        if(c.ServerGuid != Guid.Empty)
        {
            await PhotosService.DeleteAdminPendingPhotoAsync(c.ServerGuid);
        }

        _staged.Remove(c);
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
            UploadPercent = 0
        });
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadProgress(string clientGuid, double percent)
    {
        StagedPhoto c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null) return Task.CompletedTask;
        c.UploadPercent = percent;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadCompleted(string clientGuid, string serverGuid, string extension,
                                       string thumbnailBase64, long sizeBytes, int width, int height)
    {
        StagedPhoto c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null)
        {
            c = new StagedPhoto { ClientGuid = clientGuid, FileName = serverGuid };
            _staged.Add(c);
        }

        c.Status            = StagedPhotoStatus.Ready;
        c.UploadPercent     = 100;
        c.SizeBytes         = sizeBytes;
        c.Extension         = extension;
        c.ThumbnailDataUrl  = thumbnailBase64;
        c.Width             = width;
        c.Height            = height;
        if(Guid.TryParse(serverGuid, out Guid g)) c.ServerGuid = g;

        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnPhotoUploadFailed(string clientGuid, string error)
    {
        StagedPhoto c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null)
        {
            c = new StagedPhoto { ClientGuid = clientGuid, FileName = error };
            _staged.Add(c);
        }

        c.Status    = StagedPhotoStatus.Error;
        c.ErrorText = error;
        StateHasChanged();
        return Task.CompletedTask;
    }

    // ───────────────────────────── Submit / Poll / Done ─────────────────────────────

    async Task SubmitAsync()
    {
        _validationError = null;
        if(!CanSubmit())
        {
            _validationError =
                L["Pick a license and add at least one successfully-uploaded image before uploading."].Value;
            return;
        }

        var request = new AdminSoundSynthPhotoBatchCommitRequestDto
        {
            SoundSynthId = SoundSynthId,
            LicenseId    = _selectedLicense.Id ?? 0,
            Items = _staged
                    .Where(s => s.Status == StagedPhotoStatus.Ready)
                    .Select(s => new AdminSoundSynthPhotoBatchCommitItemDto
                     {
                         PendingId = s.ServerGuid,
                         Source    = string.IsNullOrWhiteSpace(s.Source) ? null : s.Source.Trim()
                     })
                    .ToList()
        };

        var (job, error) = await PhotosService.CommitAdminBatchAsync(request);
        if(job is null || job.JobId is null)
        {
            _validationError = string.IsNullOrWhiteSpace(error) ? L["Failed to start batch upload."].Value : error;
            return;
        }

        _jobId            = job.JobId;
        _jobTotal         = job.Total ?? request.Items.Count;
        _jobProcessed     = job.Processed ?? 0;
        _currentPendingId = job.CurrentPendingId;
        _jobResults       = job.Results ?? new List<AdminSoundSynthPhotoBatchJobItemResultDto>();
        _phase            = Phase.Committing;
        StateHasChanged();

        _pollCts = new CancellationTokenSource();
        _ = PollLoopAsync(_pollCts.Token);
    }

    async Task PollLoopAsync(CancellationToken ct)
    {
        try
        {
            while(!ct.IsCancellationRequested && _jobId.HasValue)
            {
                await Task.Delay(750, ct);
                AdminSoundSynthPhotoBatchJobStatusDto status =
                    await PhotosService.GetAdminBatchStatusAsync(_jobId.Value);
                if(status is null) continue;

                _jobProcessed     = status.Processed ?? _jobProcessed;
                _jobTotal         = status.Total     ?? _jobTotal;
                _currentPendingId = status.CurrentPendingId;
                _jobResults       = status.Results ?? new List<AdminSoundSynthPhotoBatchJobItemResultDto>();
                await InvokeAsync(StateHasChanged);

                int state = status.State ?? 0;
                // 2 = Completed, 3 = Failed (matches BatchJobState enum in DTO).
                if(state == 2 || state == 3)
                {
                    _phase = Phase.Done;
                    await InvokeAsync(StateHasChanged);
                    break;
                }
            }
        }
        catch(TaskCanceledException) { /* ignored */ }
    }

    Task OnDoneAsync()
    {
        MudDialog.Close(DialogResult.Ok(_jobResults.Count(r => r.Succeeded ?? false)));
        return Task.CompletedTask;
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
                L["You have staged images that have not been uploaded. Closing this dialog will delete them. Continue?"].Value);
        }
        catch { confirmed = false; }

        if(!confirmed) return;

        foreach(StagedPhoto c in _staged.ToList())
        {
            if(c.Status == StagedPhotoStatus.Uploading)
            {
                try { await JS.InvokeVoidAsync("MarechaiSoundSynthPhotoBatchUpload.cancelUpload", c.ClientGuid); }
                catch { /* ignored */ }
            }

            if(c.ServerGuid != Guid.Empty)
            {
                try { await PhotosService.DeleteAdminPendingPhotoAsync(c.ServerGuid); } catch { /* ignored */ }
            }
        }

        _staged.Clear();
        MudDialog.Cancel();
    }

    public async ValueTask DisposeAsync()
    {
        try { _pollCts?.Cancel(); } catch { /* ignored */ }
        _pollCts?.Dispose();
        _dotNetRef?.Dispose();
        await Task.CompletedTask;
    }

    static string FormatCardSubtitle(StagedPhoto c)
    {
        string size = c.SizeBytes < 1024
                          ? $"{c.SizeBytes} B"
                          : c.SizeBytes < 1024 * 1024
                              ? $"{c.SizeBytes / 1024d:0.0} KB"
                              : $"{c.SizeBytes / (1024d * 1024d):0.0} MB";

        if(c.Width > 0 && c.Height > 0) return $"{size} \u2022 {c.Width}\u00d7{c.Height}";
        return size;
    }

    sealed class StagedPhoto
    {
        public string            ClientGuid       { get; init; }
        public string            FileName         { get; set; }
        public long              SizeBytes        { get; set; }
        public string            Extension        { get; set; }
        public Guid              ServerGuid       { get; set; }
        public StagedPhotoStatus Status           { get; set; }
        public double            UploadPercent    { get; set; }
        public string            ErrorText        { get; set; }
        public string            Source           { get; set; }
        public string            ThumbnailDataUrl { get; set; }
        public int               Width            { get; set; }
        public int               Height           { get; set; }
    }

    enum StagedPhotoStatus
    {
        Uploading,
        Ready,
        Error
    }
}
