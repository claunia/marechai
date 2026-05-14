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

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Suggestions;

public partial class ProcessorVideoSuggestionDialog : ComponentBase
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public long   EntityId      { get; set; }
    [Parameter] public string ProcessorName { get; set; } = "";

    bool   _submitting;
    string _videoUrl;
    string _videoIdPreview;
    string _userComment;
    string _validationError;

    void OnUrlChanged(string value)
    {
        _videoUrl        = value;
        _validationError = null;

        if(string.IsNullOrWhiteSpace(value))
        {
            _videoIdPreview = null;
            return;
        }

        _videoIdPreview = YouTubeUrlParser.TryExtract(value, out string id) ? id : null;
    }

    async Task OnSubmitAsync()
    {
        _validationError = null;

        if(string.IsNullOrEmpty(_videoIdPreview))
        {
            _validationError = L["This is not a valid YouTube URL or video ID."].Value;
            return;
        }

        var values = new Dictionary<string, object>(System.StringComparer.Ordinal)
        {
            ["video_url"] = _videoUrl.Trim()
            // The server validator fetches the canonical title from YouTube oEmbed and
            // patches it into SuggestedValues before persisting — we deliberately do NOT
            // send a title field from the client.
        };

        _submitting = true;
        try
        {
            string json = JsonSerializer.Serialize(values);
            var dto = new SuggestionDto
            {
                EntityType          = (int?)SuggestionEntityType.ProcessorVideo,
                EntityId            = EntityId,
                Status              = (int?)SuggestionStatus.Pending,
                UserComment         = string.IsNullOrWhiteSpace(_userComment) ? null : _userComment.Trim(),
                SuggestedValuesJson = json
            };

            var (created, error) = await SuggestionsService.CreateAsync(dto);

            if(created is null)
            {
                _validationError = string.IsNullOrWhiteSpace(error)
                                       ? L["Failed to submit suggestion."].Value
                                       : error;
                return;
            }

            Snackbar.Add(L["Video link submitted for review. An administrator will accept or reject it."],
                         Severity.Success);
            MudDialog.Close(DialogResult.Ok(created));
        }
        finally
        {
            _submitting = false;
        }
    }

    void OnCancel() => MudDialog.Cancel();
}
