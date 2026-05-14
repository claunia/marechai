using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareSpecKeyDto
{
    /// <summary>
    ///     Canonical (English, NBSP-normalised) attribute key. ALWAYS the same regardless of the
    ///     <c>?lang=</c> query parameter — used as a filter argument by <c>/software/by-spec</c>.
    /// </summary>
    [JsonPropertyName("key")]
    [Required]
    public required string Key { get; set; }

    /// <summary>
    ///     Canonical (English) attribute values for this key. Pair with <see cref="Key" /> for any
    ///     filter / lookup that needs to round-trip back to the DB.
    /// </summary>
    [JsonPropertyName("values")]
    [Required]
    public required List<string> Values { get; set; }

    /// <summary>
    ///     Translated display label for <see cref="Key" /> (or the canonical English when no
    ///     translation exists or the request resolved to <c>"eng"</c>). Use this for UI rendering
    ///     and the canonical <see cref="Key" /> for any URL parameter / DB filter.
    /// </summary>
    [JsonPropertyName("displayKey")]
    [Required]
    public required string DisplayKey { get; set; }

    /// <summary>
    ///     Translated display values aligned positionally with <see cref="Values" />.
    ///     <c>DisplayValues[i]</c> is the translation of <c>Values[i]</c>.
    /// </summary>
    [JsonPropertyName("displayValues")]
    [Required]
    public required List<string> DisplayValues { get; set; }
}

