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
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marechai.Services;

public class TranslationService(IHttpClientFactory httpClientFactory, IConfiguration configuration,
                                ILogger<TranslationService> logger)
{
    sealed record LanguageInfo(string NllbCode, string EnglishName);

    static readonly Dictionary<string, LanguageInfo> _languages = new()
    {
        { "eng", new LanguageInfo("eng_Latn", "English") },
        { "spa", new LanguageInfo("spa_Latn", "Spanish") },
        { "deu", new LanguageInfo("deu_Latn", "German") },
        { "fra", new LanguageInfo("fra_Latn", "French") },
        { "ita", new LanguageInfo("ita_Latn", "Italian") },
        { "lat", new LanguageInfo("lat_Latn", "Latin") },
        { "por", new LanguageInfo("por_Latn", "Portuguese") }
    };

    bool IsOpenAIConfigured => !string.IsNullOrWhiteSpace(configuration["OpenAI:Url"]);

    bool IsNllbConfigured => !string.IsNullOrWhiteSpace(configuration["NllbServe:Url"]);

    public bool IsAvailable => IsOpenAIConfigured || IsNllbConfigured;

    public static bool IsLanguageSupported(string iso639_3) =>
        iso639_3 is not null && _languages.ContainsKey(iso639_3);

    public async Task<(string translatedText, string error)> TranslateAsync(string text,
        string targetLanguageIso639_3, IProgress<(int current, int total)> progress = null)
    {
        if(!IsAvailable)
            return (null, "Translation server is not configured.");

        if(!_languages.TryGetValue(targetLanguageIso639_3, out LanguageInfo target))
            return (null, $"Language '{targetLanguageIso639_3}' is not supported for translation.");

        // OpenAI takes precedence; on any failure we fall through to NLLB if configured.
        if(IsOpenAIConfigured)
        {
            (string openAiText, string openAiError) =
                await TranslateViaOpenAIAsync(text, target.EnglishName, progress);

            if(openAiText is not null)
                return (openAiText, null);

            if(IsNllbConfigured)
            {
                logger.LogWarning("OpenAI translation failed ({Error}); falling back to NLLB.", openAiError);

                return await TranslateViaNllbAsync(text, target.NllbCode, progress);
            }

            return (null, openAiError);
        }

        return await TranslateViaNllbAsync(text, target.NllbCode, progress);
    }

#region OpenAI backend

    async Task<(string translated, string error)> TranslateViaOpenAIAsync(string text, string targetEnglishName,
        IProgress<(int current, int total)> progress)
    {
        progress?.Report((0, 1));

        try
        {
            HttpClient client = httpClientFactory.CreateClient("OpenAI");

            string systemPrompt =
                $"You are a translator. Translate the user's text from English to {targetEnglishName}, "
              + "preserving all markdown formatting exactly (headings, lists, links, code spans, fenced code "
              + "blocks, tables, emphasis). Output ONLY the translated markdown — no preamble, no explanation, "
              + "no surrounding code fence.";

            // Build the body as a Dictionary so optional fields (model, max_tokens) can be omitted entirely
            // when not configured, which matches what local OpenAI-compatible servers expect.
            var body = new Dictionary<string, object>
            {
                ["messages"] = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content   = text ?? string.Empty }
                },
                ["temperature"] = 0
            };

            string model = configuration["OpenAI:Model"];

            if(!string.IsNullOrWhiteSpace(model))
                body["model"] = model;

            if(int.TryParse(configuration["OpenAI:MaxTokens"], out int maxTokens) && maxTokens > 0)
                body["max_tokens"] = maxTokens;

            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
            {
                Content = JsonContent.Create(body)
            };

            string apiKey = configuration["OpenAI:ApiKey"];

            if(!string.IsNullOrWhiteSpace(apiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead,
                                                                  CancellationToken.None);

            if(!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                logger.LogError("OpenAI returned {StatusCode}: {Body}", response.StatusCode, errorBody);

                return (null, $"OpenAI translation failed (HTTP {(int)response.StatusCode}).");
            }

            OpenAIChatResponse result;

            try
            {
                result = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>();
            }
            catch(JsonException ex)
            {
                logger.LogError(ex, "Failed to parse OpenAI response.");

                return (null, "OpenAI returned an unparseable response.");
            }

            string content = result?.Choices?.FirstOrDefault()?.Message?.Content;

            if(string.IsNullOrWhiteSpace(content))
                return (null, "OpenAI returned an empty response.");

            content = content.Trim();

            // Defensive: if a noncompliant model wrapped the whole reply in a single outer fence,
            // peel it off. Skip when the payload contains inner fenced blocks.
            string peeled = TryStripOuterFence(content);

            if(peeled is not null)
                content = peeled;

            // Cheap refusal detection: very short replies starting with refusal phrases when the
            // input was substantially longer should be treated as a failure so we can fall back.
            if(LooksLikeRefusal(content, text))
            {
                logger.LogWarning("OpenAI returned a refusal-shaped response ({Length} chars).", content.Length);

                return (null, "OpenAI refused or could not translate the request.");
            }

            progress?.Report((1, 1));

            return (content, null);
        }
        catch(HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to connect to OpenAI server.");

            return (null, "Could not connect to the OpenAI server.");
        }
        catch(TaskCanceledException)
        {
            return (null, "OpenAI translation request timed out.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Unexpected error during OpenAI translation.");

            return (null, "An unexpected error occurred during OpenAI translation.");
        }
    }

    /// <summary>
    ///     If the trimmed content is wrapped in a single outer triple-backtick fence (with no inner fences),
    ///     returns the inner content trimmed. Otherwise returns null.
    /// </summary>
    static string TryStripOuterFence(string content)
    {
        if(string.IsNullOrEmpty(content)) return null;

        if(!content.StartsWith("```", StringComparison.Ordinal) ||
           !content.EndsWith("```",   StringComparison.Ordinal))
            return null;

        // Must be exactly two fences (opening + closing). Three or more means inner fenced blocks.
        int fenceCount = 0;
        int searchPos  = 0;

        while((searchPos = content.IndexOf("```", searchPos, StringComparison.Ordinal)) >= 0)
        {
            fenceCount++;
            searchPos += 3;
        }

        if(fenceCount != 2) return null;

        // Strip the opening fence and any same-line language tag.
        int firstNewline = content.IndexOf('\n');
        if(firstNewline < 0) return null;

        int innerStart = firstNewline + 1;

        // Strip the trailing fence (and any whitespace/newline before it).
        int innerEnd = content.LastIndexOf("```", StringComparison.Ordinal);
        if(innerEnd <= innerStart) return null;

        return content.Substring(innerStart, innerEnd - innerStart).Trim();
    }

    static bool LooksLikeRefusal(string output, string input)
    {
        if(string.IsNullOrEmpty(output) || string.IsNullOrEmpty(input)) return false;

        // Only treat as refusal when the output is tiny relative to the input; otherwise long
        // legitimate translations starting with these phrases would be falsely rejected.
        if(output.Length >= 200 || output.Length * 4 >= input.Length) return false;

        string head = output.TrimStart().AsSpan(0, Math.Min(40, output.TrimStart().Length)).ToString();

        return head.StartsWith("I cannot",   StringComparison.OrdinalIgnoreCase) ||
               head.StartsWith("I can't",    StringComparison.OrdinalIgnoreCase) ||
               head.StartsWith("I'm sorry",  StringComparison.OrdinalIgnoreCase) ||
               head.StartsWith("I am sorry", StringComparison.OrdinalIgnoreCase) ||
               head.StartsWith("Sorry,",     StringComparison.OrdinalIgnoreCase) ||
               head.StartsWith("As an AI",   StringComparison.OrdinalIgnoreCase);
    }

    sealed class OpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAIChoice> Choices { get; set; }
    }

    sealed class OpenAIChoice
    {
        [JsonPropertyName("message")]
        public OpenAIMessage Message { get; set; }
    }

    sealed class OpenAIMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

#endregion

#region NLLB backend

    async Task<(string translatedText, string error)> TranslateViaNllbAsync(string text, string targetCode,
        IProgress<(int current, int total)> progress)
    {
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
                    (string translated, string error) = await TranslateNllbChunkAsync(client, line, targetCode);

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

    async Task<(string translated, string error)> TranslateNllbChunkAsync(HttpClient client, string chunk,
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

#endregion
}
