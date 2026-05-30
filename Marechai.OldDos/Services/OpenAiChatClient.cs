using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Marechai.OldDos.Services;

/// <summary>
///     Thin HTTP client for OpenAI-compatible chat completion endpoints. Used only by the OldDos
///     enrich pipeline. We do NOT depend on Marechai.Translation here because the prompts are
///     domain-specific (literal Russian→English, museum-grade rewrite, category picker with JSON
///     schema response). Marechai.Translation is reserved for the 5-locale fan-out at accept time.
/// </summary>
public sealed class OpenAiChatClient : IDisposable
{
    readonly HttpClient _client;
    readonly string     _model;
    readonly int?       _maxTokens;

    public OpenAiChatClient(string baseUrl, string apiKey, string model, int timeoutSeconds, int? maxTokens)
    {
        if(string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("OpenAI:Url is required.", nameof(baseUrl));
        if(string.IsNullOrWhiteSpace(model))   throw new ArgumentException("OpenAI:Model is required.", nameof(model));

        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout     = TimeSpan.FromSeconds(timeoutSeconds <= 0 ? 600 : timeoutSeconds)
        };
        if(!string.IsNullOrEmpty(apiKey))
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        _model     = model;
        _maxTokens = maxTokens;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, object jsonSchema = null)
    {
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt },
            new { role = "user",   content = userPrompt }
        };

        object body = jsonSchema != null
            ? new
              {
                  model       = _model,
                  messages    = messages,
                  max_tokens  = _maxTokens,
                  response_format = new
                  {
                      type        = "json_schema",
                      json_schema = jsonSchema
                  }
              }
            : (object)new { model = _model, messages = messages, max_tokens = _maxTokens };

        using HttpResponseMessage resp = await _client.PostAsJsonAsync("chat/completions", body);
        string text = await resp.Content.ReadAsStringAsync();
        if(!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"OpenAI {(int)resp.StatusCode}: {text}");

        using JsonDocument doc = JsonDocument.Parse(text);
        return ExtractContent(doc.RootElement)
            ?? throw new InvalidOperationException(
                "OpenAI response had no recognizable content field. Raw payload: " +
                (text.Length > 1500 ? text.Substring(0, 1500) + "…" : text));
    }

    /// <summary>
    ///     Walk an OpenAI-compatible chat-completion response and return the assistant message
    ///     content. Tolerates several shapes seen in the wild:
    ///     <list type="bullet">
    ///         <item>Standard OpenAI: <c>choices[0].message.content</c> as a string.</item>
    ///         <item>gpt-oss / some LM Studio configs: <c>choices[0].message.content</c> may be
    ///               missing or null when the model emitted only <c>reasoning_content</c>; we fall
    ///               back to <c>reasoning_content</c>, then <c>reasoning</c>.</item>
    ///         <item>Content-as-array (some Anthropic-bridge proxies): <c>content</c> is an array
    ///               of <c>{type:"text", text:"..."}</c> chunks — we concatenate the text parts.</item>
    ///         <item>Top-level <c>error</c>: surfaced as the message.</item>
    ///     </list>
    /// </summary>
    static string ExtractContent(JsonElement root)
    {
        if(root.TryGetProperty("error", out JsonElement err))
            throw new InvalidOperationException("OpenAI error: " + err);

        if(!root.TryGetProperty("choices", out JsonElement choices) ||
           choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
            return null;

        JsonElement first = choices[0];
        if(!first.TryGetProperty("message", out JsonElement message)) return null;

        if(message.TryGetProperty("content", out JsonElement content))
        {
            string s = ContentToString(content);
            if(!string.IsNullOrWhiteSpace(s)) return s;
        }

        if(message.TryGetProperty("reasoning_content", out JsonElement reasoning) &&
           ContentToString(reasoning) is { Length: > 0 } r1)
            return r1;
        if(message.TryGetProperty("reasoning", out JsonElement reasoning2) &&
           ContentToString(reasoning2) is { Length: > 0 } r2)
            return r2;

        return null;
    }

    static string ContentToString(JsonElement el)
    {
        switch(el.ValueKind)
        {
            case JsonValueKind.String: return el.GetString();
            case JsonValueKind.Array:
            {
                var sb = new System.Text.StringBuilder();
                foreach(JsonElement chunk in el.EnumerateArray())
                {
                    if(chunk.ValueKind == JsonValueKind.String) sb.Append(chunk.GetString());
                    else if(chunk.ValueKind == JsonValueKind.Object &&
                            chunk.TryGetProperty("text", out JsonElement t) &&
                            t.ValueKind == JsonValueKind.String)
                        sb.Append(t.GetString());
                }
                return sb.ToString();
            }
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                return el.ToString();
        }
    }

    public void Dispose() => _client?.Dispose();
}
