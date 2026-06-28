#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.ApiClient;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.Models;
using Marechai.Data;
using Microsoft.Extensions.Localization;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public sealed class SuggestionsService(Client client, IStringLocalizer localizer)
{
    public async Task<(List<SuggestionListItem> Suggestions, string? Error)> GetQueueAsync(
        bool includeHistory = false, int page = 1, int pageSize = 50)
    {
        try
        {
            List<SuggestionDto>? suggestions = await client.Suggestions.Queue.GetAsync(c =>
            {
                c.QueryParameters.IncludeHistory = includeHistory;
                c.QueryParameters.Page           = page;
                c.QueryParameters.PageSize       = pageSize;
            });

            return (suggestions?.Select(ToListItem).ToList() ?? [], null);
        }
        catch(ApiException ex)
        {
            return ([], ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return ([], ex.Message);
        }
    }

    public async Task<(SuggestionListItem? Header, List<SuggestionFieldDiffItem> Fields, string? UserComment,
        bool EntityMissing, string? Error)> GetDiffAsync(long id)
    {
        try
        {
            SuggestionDiffDto? diff = await client.Suggestions[id].Diff.GetAsync();

            if(diff?.Suggestion is null) return (null, [], null, false, "Suggestion not found.");

            Dictionary<string, string> currentLabels   = ExtractLabels(diff.CurrentLabels?.AdditionalData);
            Dictionary<string, string> suggestedLabels = ExtractLabels(diff.SuggestedLabels?.AdditionalData);
            Dictionary<string, JsonElement> currentValues   = ParseValuesJson(diff.CurrentValuesJson);
            Dictionary<string, JsonElement> suggestedValues = ParseValuesJson(diff.Suggestion.SuggestedValuesJson);

            List<SuggestionFieldDiffItem> fields = suggestedValues.Select(kv => new SuggestionFieldDiffItem
            {
                FieldName      = kv.Key,
                CurrentLabel   = currentLabels.TryGetValue(kv.Key, out string? cl) ? cl : ValueToText(currentValues, kv.Key),
                SuggestedLabel = suggestedLabels.TryGetValue(kv.Key, out string? sl) ? sl : ValueToText(kv.Value)
            }).ToList();

            return (ToListItem(diff.Suggestion), fields, diff.Suggestion.UserComment, diff.EntityMissing == true, null);
        }
        catch(ApiException ex)
        {
            return (null, [], null, false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, [], null, false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? Error)> ReviewAsync(long id, List<string> acceptedFieldNames,
        string? adminComment)
    {
        try
        {
            await client.Suggestions[id].Review.PostAsync(new SuggestionReviewDto
            {
                AcceptedFieldNames = acceptedFieldNames,
                AdminComment       = string.IsNullOrWhiteSpace(adminComment) ? null : adminComment.Trim()
            });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    SuggestionListItem ToListItem(SuggestionDto dto)
    {
        var status = (SuggestionStatus)(dto.Status ?? 0);

        return new SuggestionListItem
        {
            Id                = dto.Id ?? 0,
            EntityTypeText    = ((SuggestionEntityType)(dto.EntityType ?? 0)).ToString(),
            EntityDisplayName = dto.EntityDisplayName ?? string.Empty,
            EntityId          = dto.EntityId,
            IsNew             = dto.EntityId is null,
            SubkeyText        = dto.Subkey ?? string.Empty,
            CreatedByText     = dto.CreatedByDisplayName ?? dto.CreatedByUserName ?? string.Empty,
            CreatedOnText     = dto.CreatedOn?.LocalDateTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
            StatusText        = StatusLabel(status),
            IsPending         = status == SuggestionStatus.Pending
        };
    }

    string StatusLabel(SuggestionStatus status) => status switch
    {
        SuggestionStatus.Pending           => localizer["SuggestionsStatusPending"],
        SuggestionStatus.Accepted          => localizer["SuggestionsStatusAccepted"],
        SuggestionStatus.PartiallyAccepted => localizer["SuggestionsStatusPartiallyAccepted"],
        SuggestionStatus.Rejected          => localizer["SuggestionsStatusRejected"],
        SuggestionStatus.Stale             => localizer["SuggestionsStatusStale"],
        SuggestionStatus.Withdrawn         => localizer["SuggestionsStatusWithdrawn"],
        _                                  => status.ToString()
    };

    static Dictionary<string, string> ExtractLabels(IDictionary<string, object>? additionalData)
    {
        var result = new Dictionary<string, string>();
        if(additionalData is null) return result;

        foreach(KeyValuePair<string, object> kv in additionalData)
            result[kv.Key] = kv.Value switch
            {
                null   => string.Empty,
                string s => s,
                _      => kv.Value.ToString() ?? string.Empty
            };

        return result;
    }

    static Dictionary<string, JsonElement> ParseValuesJson(string? json)
    {
        if(string.IsNullOrWhiteSpace(json)) return [];

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
        }
        catch(JsonException)
        {
            return [];
        }
    }

    static string ValueToText(Dictionary<string, JsonElement> values, string key) =>
        values.TryGetValue(key, out JsonElement element) ? ValueToText(element) : string.Empty;

    static string ValueToText(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Null   => string.Empty,
        _                    => element.ToString()
    };

    static string ExtractErrorMessage(ApiException ex) =>
        ex is ProblemDetails pd ? pd.Detail ?? pd.Title ?? ex.Message : ex.Message;
}
