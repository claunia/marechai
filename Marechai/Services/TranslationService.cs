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

            // Tokenize the text into alternating (line, separator) pairs so we can translate each
            // line independently and re-assemble the text with the exact original separators.
            // Recognised line separators (in priority order):
            //   CRLF (\r\n), LF (\n), CR (\r), NEL (U+0085), LS (U+2028), PS (U+2029),
            //   VT (U+000B), FF (U+000C).
            // This handles Unix, Windows, classic Mac, and Unicode-rich text equally well.
            List<(string line, string separator)> tokens = TokenizeLines(text);

            // Count non-empty lines for progress
            int totalChunks     = tokens.Count(t => !string.IsNullOrWhiteSpace(t.line));
            int completedChunks = 0;

            progress?.Report((0, totalChunks));

            var translatedParts = new StringBuilder();

            foreach((string line, string separator) in tokens)
            {
                if(string.IsNullOrWhiteSpace(line))
                    translatedParts.Append(line);
                else
                {
                    (string translated, string error) = await TranslateChunkAsync(client, line, targetCode);

                    if(error is not null)
                        return (null, error);

                    translatedParts.Append(translated);

                    completedChunks++;
                    progress?.Report((completedChunks, totalChunks));
                }

                translatedParts.Append(separator);
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

    static List<(string line, string separator)> TokenizeLines(string text)
    {
        var tokens = new List<(string line, string separator)>();

        if(string.IsNullOrEmpty(text))
        {
            tokens.Add((text ?? string.Empty, string.Empty));

            return tokens;
        }

        int start = 0;

        for(int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // Recognised line separators (priority order matters for CRLF)
            if(c is not ('\r' or '\n' or '\u0085' or '\u2028' or '\u2029' or '\u000B' or '\u000C'))
                continue;

            string separator;

            // CRLF must be treated as a single separator
            if(c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
                separator = "\r\n";
                i++;
            }
            else
                separator = c.ToString();

            string line = text.Substring(start, i - separator.Length + 1 - start);
            tokens.Add((line, separator));
            start = i + 1;
        }

        // Trailing line without a final separator
        if(start <= text.Length)
            tokens.Add((text[start..], string.Empty));

        return tokens;
    }

    async Task<(string translated, string error)> TranslateChunkAsync(HttpClient client, string chunk,
                                                                        string targetCode)
    {        var formData = new FormUrlEncodedContent(
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
