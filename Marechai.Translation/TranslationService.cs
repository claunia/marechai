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

namespace Marechai.Translation;

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
        { "por", new LanguageInfo("por_Latn", "Portuguese") }
    };

    bool IsOpenAIConfigured => !string.IsNullOrWhiteSpace(configuration["OpenAI:Url"]);

    bool IsNllbConfigured => !string.IsNullOrWhiteSpace(configuration["NllbServe:Url"]);

    public bool IsAvailable => IsOpenAIConfigured || IsNllbConfigured;

    public static bool IsLanguageSupported(string iso639_3) =>
        iso639_3 is not null && _languages.ContainsKey(iso639_3);

    /// <summary>
    ///     The set of ISO 639-3 codes the translator can target. <c>eng</c> is included as the source
    ///     identity. Worker code that needs to enumerate target languages should use this property and
    ///     filter out <c>eng</c> itself.
    /// </summary>
    public static IReadOnlyCollection<string> SupportedLanguageCodes => _languages.Keys;

    public async Task<(string translatedText, string error)> TranslateAsync(string text,
        string targetLanguageIso639_3, IProgress<(int current, int total)> progress = null,
        bool plainText = false, string domainContext = null)
    {
        if(!IsAvailable)
            return (null, "Translation server is not configured.");

        if(!_languages.TryGetValue(targetLanguageIso639_3, out LanguageInfo target))
            return (null, $"Language '{targetLanguageIso639_3}' is not supported for translation.");

        // Normalise non-breaking space (U+00A0) to regular ASCII space BEFORE sending to either
        // backend. MobyGames-imported attribute strings carry NBSPs that confuse OpenAI on short
        // labels (e.g. "Minimum\u00A0RAM\u00A0Required" gets back-translated literally with the
        // NBSPs preserved, defeating cache lookups downstream). Other Unicode line/paragraph
        // separators flow through unchanged — only NBSP is normalised here.
        if(!string.IsNullOrEmpty(text) && text.IndexOf('\u00A0') >= 0)
            text = text.Replace('\u00A0', ' ');

        // OpenAI takes precedence; on any failure we fall through to NLLB if configured.
        if(IsOpenAIConfigured)
        {
            (string openAiText, string openAiError) =
                await TranslateViaOpenAIAsync(text, target.EnglishName, progress, plainText, domainContext);

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
        IProgress<(int current, int total)> progress, bool plainText, string domainContext = null)
    {
        progress?.Report((0, 1));

        try
        {
            HttpClient client = httpClientFactory.CreateClient("OpenAI");

            string model = configuration["OpenAI:Model"];

            // Optional separate model for markdown translations (e.g. a non-thinking model that
            // handles verbose formatting instructions without entering a reasoning loop).
            // When set, markdown translations use this model + the verbose prompt; when null,
            // we auto-detect thinking models by name and fall back to a minimal prompt.
            string markdownModel = configuration["OpenAI:MarkdownModel"];

            // Effective model for this request: use the markdown-specific model for non-plainText
            // when configured, otherwise use the default model.
            string effectiveModel = !plainText && !string.IsNullOrWhiteSpace(markdownModel)
                                        ? markdownModel
                                        : model;

            bool isThinkingModel = effectiveModel is not null &&
                                   effectiveModel.Contains("qwen", StringComparison.OrdinalIgnoreCase);

            string systemPrompt;

            if(plainText)
            {
                systemPrompt = $"You are a translator. Translate the user's text from English to {targetEnglishName}. "
                             + "The input is plain text (typically a short label, never markdown). DO NOT add any "
                             + "formatting characters (#, *, _, `, -, >). Output ONLY a JSON object of the form "
                             + "{\"translation\":\"...\"} containing the translated text, nothing else.";
            }
            else if(isThinkingModel)
            {
                // Thinking models (Qwen 3.x) enter an unbounded reasoning loop when given verbose
                // markdown-preservation instructions — even with /no_think and json_schema. A
                // minimal prompt avoids this while still requesting formatting preservation.
                systemPrompt = $"You are a translator. Translate the user's text from English to {targetEnglishName}. "
                             + "Keep all markdown formatting unchanged. Output ONLY the translated text — "
                             + "no preamble, no explanation, no surrounding code fence.";
            }
            else
            {
                // Non-thinking models handle verbose instructions well and benefit from the
                // explicit enumeration of markdown elements to preserve.
                systemPrompt = $"You are a translator. Translate the user's text from English to {targetEnglishName}, "
                             + "preserving all markdown formatting exactly (headings, lists, links, code spans, fenced "
                             + "code blocks, tables, emphasis). Output ONLY the translated markdown — no preamble, no "
                             + "explanation, no surrounding code fence.";
            }

            // Optional caller-supplied domain hint (e.g. "The text is a software genre name." or
            // "The text is a video-game technical specification key or value (computer/console
            // hardware terminology)."). Steers the model away from over-generic translations on
            // short labels — without this, e.g. "Mouse" gets translated as the animal in some
            // languages instead of the input device.
            //
            // Skip for thinking models: verbose domain hints (especially "Keep brand names,
            // character names..." combined with proper nouns in the input) trigger Qwen 3.x into
            // an unbounded reasoning loop about what to preserve vs. translate.
            if(!isThinkingModel && !string.IsNullOrWhiteSpace(domainContext))
                systemPrompt += " Context: " + domainContext.Trim();

            // Qwen 3.x models have "thinking" enabled by default and can burn 60k+ tokens of
            // internal reasoning before producing a one-line translation. The /no_think directive
            // at the start of the system message disables this at the chat-template level,
            // regardless of serving backend (vLLM, LM Studio, etc.).
            bool disableThinking = string.Equals(configuration["OpenAI:DisableThinking"], "true",
                                                 StringComparison.OrdinalIgnoreCase);

            if(disableThinking)
                systemPrompt = "/no_think\n" + systemPrompt;

            // Build the body as a Dictionary so optional fields (model, max_tokens, response_format)
            // can be omitted entirely when not needed, which matches what local OpenAI-compatible
            // servers expect.
            var body = new Dictionary<string, object>
            {
                ["messages"] = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content   = text ?? string.Empty }
                },
                ["temperature"] = 0
            };

            // Also pass the vLLM / LM Studio extension that disables thinking at the engine level,
            // for servers that support it (silently ignored by others).
            if(disableThinking)
                body["chat_template_kwargs"] = new { enable_thinking = false };

            if(plainText)
            {
                // Force the model to emit a strictly-typed JSON object with a single `translation`
                // string. This is CRITICAL for reasoning models (Qwen 3.x on LM Studio) — without
                // json_schema, they enter an unbounded thinking loop. With it, output is capped to
                // the actual translation.
                //
                // NOTE: json_schema MUST NOT be used for markdown translations. Qwen 3.x enters an
                // infinite reasoning loop when markdown syntax is present in the user message AND
                // json_schema is active — regardless of prompt wording. For markdown, we rely on
                // unconstrained output + HTTP timeout + NLLB fallback.
                body["response_format"] = new
                {
                    type        = "json_schema",
                    json_schema = new
                    {
                        name   = "translation",
                        strict = true,
                        schema = new
                        {
                            type                 = "object",
                            additionalProperties = false,
                            required             = new[] { "translation" },
                            properties = new
                            {
                                translation = new { type = "string" }
                            }
                        }
                    }
                };
            }

            if(!string.IsNullOrWhiteSpace(effectiveModel))
                body["model"] = effectiveModel;

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

            OpenAIMessage message = result?.Choices?.FirstOrDefault()?.Message;
            string        content = message?.Content;

            // Some OpenAI-compatible servers (notably LM Studio serving the Qwen3 family) place
            // structured-output (`response_format: json_schema`) and even regular reply text in
            // `reasoning_content` rather than `content`, leaving `content` an empty string. The
            // OldDos OpenAiChatClient already does this fallback; mirror that here so translation
            // works against reasoning models without per-model branching.
            if(string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(message?.ReasoningContent))
                content = message.ReasoningContent;

            if(string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(message?.Reasoning))
                content = message.Reasoning;

            if(string.IsNullOrWhiteSpace(content))
                return (null, "OpenAI returned an empty response.");

            content = content.Trim();

            if(plainText)
            {
                // Parse the {"translation": "..."} envelope; reject anything else.
                string parsed = TryExtractTranslationField(content);

                if(parsed is null)
                {
                    logger.LogWarning(
                        "OpenAI plain-text translation returned a non-conforming response (first 80 chars): {Head}",
                        content.Length > 80 ? content[..80] : content);

                    return (null, "OpenAI returned a non-JSON response in plain-text mode.");
                }

                content = parsed.Trim();

                // Defensive scrub for any leftover markdown leader/trailer that slipped past JSON
                // parsing (e.g. the model returning {"translation":"# Acción"}).
                content = StripMarkdownDecoration(content);
            }
            else
            {
                // Some models wrap the translation in a JSON envelope even when not asked to.
                // Try to extract from {"translation":"..."} first; fall back to raw content.
                string fromJson = TryExtractTranslationField(content);

                if(fromJson is not null)
                {
                    content = fromJson.Trim();
                }
                else
                {
                    // Defensive: if a noncompliant model wrapped the whole reply in a single outer
                    // fence, peel it off. Skip when the payload contains inner fenced blocks.
                    string peeled = TryStripOuterFence(content);

                    if(peeled is not null)
                        content = peeled;
                }
            }

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
    ///     Extracts the <c>translation</c> string field from a JSON object reply. Returns null when the
    ///     payload doesn't contain a JSON object with a recognised string field. Tolerates a wider set
    ///     of common envelope keys that some models default to (translated, output, result, text) so a
    ///     small variation in model behaviour doesn't break the whole pipeline.
    ///     <para>
    ///         The extractor is lenient about what surrounds the JSON object — reasoning-channel output
    ///         from Qwen3 / DeepSeek / similar models routinely arrives with:
    ///         <list type="bullet">
    ///             <item>A leading <c>```json</c> code fence (and trailing <c>```</c>).</item>
    ///             <item>Trailing prose after the closing brace (e.g. "Final answer: …").</item>
    ///             <item>A "thinking" preamble before the first <c>{</c>.</item>
    ///         </list>
    ///         We scan for the first balanced <c>{…}</c> substring (respecting string literals and
    ///         escapes), parse that, and ignore everything outside it. Falls back to <c>last-brace</c>
    ///         heuristic when balance walking fails (truncated reply).
    ///     </para>
    /// </summary>
    static string TryExtractTranslationField(string content)
    {
        if(string.IsNullOrWhiteSpace(content)) return null;

        // Strip a wrapping ```json … ``` fence if present.
        string scan = content.Trim();
        if(scan.StartsWith("```", StringComparison.Ordinal))
        {
            int firstNewline = scan.IndexOf('\n');
            int lastFence    = scan.LastIndexOf("```", StringComparison.Ordinal);
            if(firstNewline > 0 && lastFence > firstNewline)
                scan = scan.Substring(firstNewline + 1, lastFence - firstNewline - 1).Trim();
        }

        // 1) Try strict parse first — cheapest path when the model behaves.
        string s = TryParseAndExtract(scan);
        if(s is not null) return s;

        // 2) Find the first balanced {…} substring and parse that.
        string balanced = ExtractFirstBalancedObject(scan);
        if(balanced is not null && balanced.Length != scan.Length)
        {
            s = TryParseAndExtract(balanced);
            if(s is not null) return s;
        }

        // 3) Last-resort: truncated reply (model ran out of tokens mid-JSON). Try slicing from the
        //    first '{' to the LAST '}' and parsing.
        int firstBrace = scan.IndexOf('{');
        int lastBrace  = scan.LastIndexOf('}');
        if(firstBrace >= 0 && lastBrace > firstBrace)
        {
            string slice = scan.Substring(firstBrace, lastBrace - firstBrace + 1);
            s = TryParseAndExtract(slice);
            if(s is not null) return s;
        }

        return null;

        static string TryParseAndExtract(string text)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(text);
                if(doc.RootElement.ValueKind != JsonValueKind.Object) return null;

                foreach(string key in (string[])["translation", "translated", "output", "result", "text"])
                {
                    if(doc.RootElement.TryGetProperty(key, out JsonElement val) &&
                       val.ValueKind == JsonValueKind.String)
                        return val.GetString();
                }

                return null;
            }
            catch(JsonException)
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     Returns the substring from the first <c>{</c> to its matching <c>}</c>, correctly skipping
    ///     braces inside JSON string literals (with backslash escapes). Returns null when there is no
    ///     balanced match (e.g. truncated content).
    /// </summary>
    static string ExtractFirstBalancedObject(string text)
    {
        if(string.IsNullOrEmpty(text)) return null;

        int start = text.IndexOf('{');
        if(start < 0) return null;

        int  depth    = 0;
        bool inString = false;
        bool escape   = false;

        for(int i = start; i < text.Length; i++)
        {
            char c = text[i];

            if(escape)
            {
                escape = false;

                continue;
            }

            if(inString)
            {
                if(c == '\\')
                    escape = true;
                else if(c == '"')
                    inString = false;

                continue;
            }

            switch(c)
            {
                case '"':
                    inString = true;

                    break;
                case '{':
                    depth++;

                    break;
                case '}':
                    depth--;

                    if(depth == 0)
                        return text.Substring(start, i - start + 1);

                    break;
            }
        }

        return null;
    }

    /// <summary>
    ///     Strips leading / trailing markdown leader characters (#, *, _, `, &gt;, -) and surrounding
    ///     whitespace. Used as a safety net for plain-text translations after JSON envelope extraction.
    ///     Only strips characters at the very start / end of the string; internal characters (e.g. the
    ///     hyphen in "Beat 'em up / brawler") are left untouched.
    /// </summary>
    static string StripMarkdownDecoration(string content)
    {
        if(string.IsNullOrEmpty(content)) return content;

        ReadOnlySpan<char> span = content.AsSpan().Trim();

        int start = 0;
        while(start < span.Length && span[start] is '#' or '*' or '_' or '`' or '>') start++;

        int end = span.Length;
        while(end > start && span[end - 1] is '#' or '*' or '_' or '`') end--;

        return span[start..end].Trim().ToString();
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

        // Reasoning-model channels. Some OpenAI-compatible backends (LM Studio + Qwen3 family
        // observed in the wild) return the final answer in one of these fields instead of
        // `content`, leaving `content` blank. Read both spellings: `reasoning_content` (LM Studio,
        // vLLM) and `reasoning` (some other proxies).
        [JsonPropertyName("reasoning_content")]
        public string ReasoningContent { get; set; }

        [JsonPropertyName("reasoning")]
        public string Reasoning { get; set; }
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
