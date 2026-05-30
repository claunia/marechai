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
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareScreenshotsBatchUploadDialog : ComponentBase, IAsyncDisposable
{
    public const int MaxImages = 50;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public int                        SoftwareId            { get; set; }
    [Parameter] public string                     SoftwareName          { get; set; } = "";
    [Parameter] public List<SoftwarePlatformDto>  Platforms             { get; set; } = new();
    [Parameter] public int?                       SoftwareVersionId     { get; set; }
    [Parameter] public string                     SoftwareVersionString { get; set; } = "";

    enum Phase { Staging, Committing, Done }

    Phase               _phase = Phase.Staging;
    string              _validationError;
    SoftwarePlatformDto _selectedPlatform;
    string              _selectedGroupName;

    readonly List<StagedScreenshot> _staged      = new();
    readonly string                 _fileInputId = $"sscr-batch-input-{Guid.NewGuid():N}";

    DotNetObjectReference<SoftwareScreenshotsBatchUploadDialog> _dotNetRef;

    Guid?                                                  _jobId;
    int                                                    _jobTotal;
    int                                                    _jobProcessed;
    Guid?                                                  _currentPendingId;
    List<AdminSoftwareScreenshotBatchJobItemResultDto>     _jobResults = new();
    CancellationTokenSource                                _pollCts;

    double _jobPercent => _jobTotal <= 0 ? 0 : Math.Min(100, _jobProcessed * 100.0 / _jobTotal);

    protected override void OnInitialized()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    bool CanSubmit()
    {
        if(_staged.Count == 0) return false;
        if(_staged.Any(s => s.Status == StagedScreenshotStatus.Uploading)) return false;
        if(!_staged.All(s => s.Status == StagedScreenshotStatus.Ready)) return false;
        return true;
    }

    async Task TriggerFilePicker()
    {
        try { await JS.InvokeVoidAsync("MarechaiSoftwareScreenshotBatchUpload.clickInput", _fileInputId); }
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
            await JS.InvokeAsync<object>("MarechaiSoftwareScreenshotBatchUpload.uploadBatch",
                                         ApiAssetUrls.BaseUrl, token, SoftwareId, _fileInputId,
                                         _dotNetRef, slotsLeft);
        }
        catch(Exception ex)
        {
            _validationError = ex.Message;
        }
    }

    async Task RemoveStagedAsync(string clientGuid)
    {
        StagedScreenshot c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null) return;

        if(c.Status == StagedScreenshotStatus.Uploading)
        {
            try { await JS.InvokeVoidAsync("MarechaiSoftwareScreenshotBatchUpload.cancelUpload", c.ClientGuid); }
            catch { /* ignored */ }
        }

        if(c.ServerGuid != Guid.Empty)
        {
            await SoftwareService.DeleteAdminPendingScreenshotAsync(c.ServerGuid);
        }

        _staged.Remove(c);
        StateHasChanged();
    }

    // ───────────────────────────── JS-invokable callbacks ─────────────────────────────

    [JSInvokable]
    public Task OnScreenshotUploadStarted(string clientGuid, string fileName, long sizeBytes)
    {
        _staged.Add(new StagedScreenshot
        {
            ClientGuid    = clientGuid,
            FileName      = fileName,
            SizeBytes     = sizeBytes,
            Status        = StagedScreenshotStatus.Uploading,
            UploadPercent = 0
        });
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnScreenshotUploadProgress(string clientGuid, double percent)
    {
        StagedScreenshot c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null) return Task.CompletedTask;
        c.UploadPercent = percent;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnScreenshotUploadCompleted(string clientGuid, string serverGuid, string extension,
                                            string thumbnailBase64, long sizeBytes, int width, int height)
    {
        StagedScreenshot c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null)
        {
            c = new StagedScreenshot { ClientGuid = clientGuid, FileName = serverGuid };
            _staged.Add(c);
        }

        c.Status            = StagedScreenshotStatus.Ready;
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
    public Task OnScreenshotUploadFailed(string clientGuid, string error)
    {
        StagedScreenshot c = _staged.FirstOrDefault(x => x.ClientGuid == clientGuid);
        if(c is null)
        {
            c = new StagedScreenshot { ClientGuid = clientGuid, FileName = error };
            _staged.Add(c);
        }

        c.Status    = StagedScreenshotStatus.Error;
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
                L["Add at least one successfully-uploaded image before uploading."].Value;
            return;
        }

        var request = new AdminSoftwareScreenshotBatchCommitRequestDto
        {
            SoftwareId         = SoftwareId,
            SoftwarePlatformId = _selectedPlatform?.Id,
            SoftwareVersionId  = SoftwareVersionId,
            CanonicalGroupName = string.IsNullOrWhiteSpace(_selectedGroupName) ? null : _selectedGroupName.Trim(),
            Items = _staged
                    .Where(s => s.Status == StagedScreenshotStatus.Ready)
                    .Select(s => new AdminSoftwareScreenshotBatchCommitItemDto
                     {
                         PendingId = s.ServerGuid,
                         Caption   = string.IsNullOrWhiteSpace(s.Caption) ? null : s.Caption.Trim()
                     })
                    .ToList()
        };

        var (job, error) = await SoftwareService.CommitAdminScreenshotBatchAsync(request);
        if(job is null || job.JobId is null)
        {
            _validationError = string.IsNullOrWhiteSpace(error) ? L["Failed to start batch upload."].Value : error;
            return;
        }

        _jobId            = job.JobId;
        _jobTotal         = job.Total ?? request.Items.Count;
        _jobProcessed     = job.Processed ?? 0;
        _currentPendingId = job.CurrentPendingId;
        _jobResults       = job.Results ?? new List<AdminSoftwareScreenshotBatchJobItemResultDto>();
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
                AdminSoftwareScreenshotBatchJobStatusDto status =
                    await SoftwareService.GetAdminScreenshotBatchStatusAsync(_jobId.Value);
                if(status is null) continue;

                _jobProcessed     = status.Processed ?? _jobProcessed;
                _jobTotal         = status.Total     ?? _jobTotal;
                _currentPendingId = status.CurrentPendingId;
                _jobResults       = status.Results ?? new List<AdminSoftwareScreenshotBatchJobItemResultDto>();
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

        foreach(StagedScreenshot c in _staged.ToList())
        {
            if(c.Status == StagedScreenshotStatus.Uploading)
            {
                try { await JS.InvokeVoidAsync("MarechaiSoftwareScreenshotBatchUpload.cancelUpload", c.ClientGuid); }
                catch { /* ignored */ }
            }

            if(c.ServerGuid != Guid.Empty)
            {
                try { await SoftwareService.DeleteAdminPendingScreenshotAsync(c.ServerGuid); } catch { /* ignored */ }
            }
        }

        _staged.Clear();
        MudDialog.Cancel();
    }

    async Task<IEnumerable<string>> SearchGroupsAsync(string search, CancellationToken cancellationToken)
    {
        List<SoftwareScreenshotGroupDto> groups = await SoftwareService.GetScreenshotGroupsAsync(search);

        return groups.Select(g => g.CanonicalName)
                     .Where(n => !string.IsNullOrWhiteSpace(n))
                     .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    public async ValueTask DisposeAsync()
    {
        try { _pollCts?.Cancel(); } catch { /* ignored */ }
        _pollCts?.Dispose();
        _dotNetRef?.Dispose();
        await Task.CompletedTask;
    }

    static string FormatCardSubtitle(StagedScreenshot c)
    {
        string size = c.SizeBytes < 1024
                          ? $"{c.SizeBytes} B"
                          : c.SizeBytes < 1024 * 1024
                              ? $"{c.SizeBytes / 1024d:0.0} KB"
                              : $"{c.SizeBytes / (1024d * 1024d):0.0} MB";

        if(c.Width > 0 && c.Height > 0) return $"{size} \u2022 {c.Width}\u00d7{c.Height}";
        return size;
    }

    sealed class StagedScreenshot
    {
        public string                 ClientGuid       { get; init; }
        public string                 FileName         { get; set; }
        public long                   SizeBytes        { get; set; }
        public string                 Extension        { get; set; }
        public Guid                   ServerGuid       { get; set; }
        public StagedScreenshotStatus Status           { get; set; }
        public double                 UploadPercent    { get; set; }
        public string                 ErrorText        { get; set; }
        public string                 Caption          { get; set; }
        public string                 ThumbnailDataUrl { get; set; }
        public int                    Width            { get; set; }
        public int                    Height           { get; set; }
    }

    enum StagedScreenshotStatus
    {
        Uploading,
        Ready,
        Error
    }
}
