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
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
/// Consolidated payload for the public /gpu/{Id} view page. Replaces 6 sequential
/// HTTP round-trips (head + resolutions + machines + description + photos + videos)
/// with a single response. The head + company name + company logo are pulled in one
/// projected query (logo via inline subquery so it costs zero extra DB round-trips);
/// the description language fallback is collapsed into a single ordered query; the
/// child collections are fetched in parallel using independent <see cref="MarechaiContext"/>
/// instances from <c>IDbContextFactory</c> (DbContext is not thread-safe; sharing the
/// request-scoped context across parallel branches throws <c>InvalidOperationException</c>).
/// </summary>
public class GpuFullDto
{
    // [Required] is needed in addition to the non-nullable type so the generated
    // OpenAPI schema is a plain $ref instead of `oneOf:[null, $ref]`. With the
    // oneOf-with-null shape Kiota generates a "composed type wrapper" that
    // requires a discriminator field that doesn't exist in the JSON, leaving the
    // inner DTO un-populated. See pattern in BookFullDto.Book.
    [JsonPropertyName("gpu")]
    [Required]
    public GpuDto Gpu { get; set; }

    /// <summary>
    /// Asset GUID of the company logo to display next to the GPU header. Picked from the
    /// company's logos with the introduced-year preference applied server-side (logos with
    /// <c>Year &gt;= introducedYear</c> sort first, then ascending by year). Null when the
    /// GPU has no company or the company has no logos.
    /// </summary>
    [JsonPropertyName("company_logo")]
    public Guid? CompanyLogo { get; set; }

    /// <summary>
    /// HTML-rendered description in the requested language (with English fallback applied
    /// server-side via a single ordered query). Null at runtime when the GPU has no
    /// description in either the requested language or English.
    /// </summary>
    [JsonPropertyName("description_html")]
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Plain-text (markdown source) description in the requested language. Mirrors
    /// <see cref="DescriptionHtml"/> for callers that prefer raw markdown over rendered
    /// HTML. Null when no description was found.
    /// </summary>
    [JsonPropertyName("description_text")]
    public string? DescriptionText { get; set; }

    /// <summary>
    /// Language code of the description that was actually returned (the requested language
    /// or "eng" if the fallback fired). Null when no description was found.
    /// </summary>
    [JsonPropertyName("description_language_code")]
    public string? DescriptionLanguageCode { get; set; }

    [JsonPropertyName("resolutions")]
    [Required]
    public List<ResolutionDto> Resolutions { get; set; } = new();

    [JsonPropertyName("machines")]
    [Required]
    public List<MachineDto> Machines { get; set; } = new();

    [JsonPropertyName("photos")]
    [Required]
    public List<Guid> Photos { get; set; } = new();

    [JsonPropertyName("videos")]
    [Required]
    public List<GpuVideoDto> Videos { get; set; } = new();
}
