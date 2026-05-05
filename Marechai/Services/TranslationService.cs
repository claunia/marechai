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
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marechai.Services;

public class TranslationService(IHttpClientFactory httpClientFactory, IConfiguration configuration,
                                ILogger<TranslationService> logger)
{
    static readonly Dictionary<string, string> _nllbLangMap = new()
    {
        { "eng", "eng_Latn" },
        { "spa", "spa_Latn" },
        { "deu", "deu_Latn" },
        { "fra", "fra_Latn" },
        { "ita", "ita_Latn" },
        { "lat", "lat_Latn" },
        { "por", "por_Latn" }
    };

    public bool IsAvailable => !string.IsNullOrWhiteSpace(configuration["NllbServe:Url"]);

    public static bool IsLanguageSupported(string iso639_3) =>
        iso639_3 is not null && _nllbLangMap.ContainsKey(iso639_3);

    public async Task<(string translatedText, string error)> TranslateAsync(string text,
                                                                              string targetLanguageIso639_3,
                                                                              IProgress<(int current, int total)> progress = null)
    {
        if(!IsAvailable)
            return (null, "Translation server is not configured.");

        if(!_nllbLangMap.TryGetValue(targetLanguageIso639_3, out string targetCode))
            return (null, $"Language '{targetLanguageIso639_3}' is not supported for translation.");

        try
        {
            HttpClient client = httpClientFactory.CreateClient("NllbServe");

            // Split by paragraphs to stay under the model's 1024 token limit
            string[] paragraphs = text.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.None);

            // Count non-empty paragraphs for progress
            int totalChunks     = paragraphs.Count(p => !string.IsNullOrWhiteSpace(p));
            int completedChunks = 0;

            progress?.Report((0, totalChunks));

            var translatedParts = new StringBuilder();

            for(int i = 0; i < paragraphs.Length; i++)
            {
                if(i > 0)
                    translatedParts.Append("\n\n");

                string paragraph = paragraphs[i];

                // Preserve empty paragraphs (consecutive blank lines)
                if(string.IsNullOrWhiteSpace(paragraph))
                {
                    translatedParts.Append(paragraph);

                    continue;
                }

                (string translated, string error) = await TranslateChunkAsync(client, paragraph, targetCode);

                if(error is not null)
                    return (null, error);

                translatedParts.Append(translated);

                completedChunks++;
                progress?.Report((completedChunks, totalChunks));
            }

            return (translatedParts.ToString(), null);
        }
        catch(HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to connect to nllb-serve");

            return (null, "Could not connect to the translation server.");
        }
        catch(TaskCanceledException)
        {
            return (null, "Translation request timed out.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Unexpected error during translation");

            return (null, "An unexpected error occurred during translation.");
        }
    }

    async Task<(string translated, string error)> TranslateChunkAsync(HttpClient client, string chunk,
                                                                        string targetCode)
    {
        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("source",   chunk),
            new KeyValuePair<string, string>("src_lang", "eng_Latn"),
            new KeyValuePair<string, string>("tgt_lang", targetCode)
        ]);

        HttpResponseMessage response = await client.PostAsync("/translate", formData);

        if(!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            logger.LogError("nllb-serve returned {StatusCode}: {Body}", response.StatusCode, body);

            return (null, $"Translation failed (HTTP {(int)response.StatusCode}).");
        }

        NllbResponse result = await response.Content.ReadFromJsonAsync<NllbResponse>();

        if(result?.Translation is not { Count: > 0 })
            return (null, "Translation returned an empty result.");

        return (result.Translation[0], null);
    }

    sealed class NllbResponse
    {
        [JsonPropertyName("translation")]
        public List<string> Translation { get; set; }
    }
}
