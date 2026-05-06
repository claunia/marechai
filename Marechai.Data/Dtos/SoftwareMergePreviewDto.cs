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

using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareMergePreviewDto
{
    [JsonPropertyName("target_id")]
    public ulong TargetId { get; set; }
    [JsonPropertyName("target_name")]
    public string TargetName { get; set; }
    [JsonPropertyName("source_id")]
    public ulong SourceId { get; set; }
    [JsonPropertyName("source_name")]
    public string SourceName { get; set; }
    [JsonPropertyName("suggested_release_title")]
    public string SuggestedReleaseTitle { get; set; }
    [JsonPropertyName("versions_count")]
    public int VersionsCount { get; set; }
    [JsonPropertyName("direct_releases_count")]
    public int DirectReleasesCount { get; set; }
    [JsonPropertyName("direct_releases_without_title_count")]
    public int DirectReleasesWithoutTitleCount { get; set; }
    [JsonPropertyName("company_roles_total")]
    public int CompanyRolesTotal { get; set; }
    [JsonPropertyName("company_roles_duplicates")]
    public int CompanyRolesDuplicates { get; set; }
    [JsonPropertyName("screenshots_count")]
    public int ScreenshotsCount { get; set; }
    [JsonPropertyName("descriptions_total")]
    public int DescriptionsTotal { get; set; }
    [JsonPropertyName("descriptions_duplicates")]
    public int DescriptionsDuplicates { get; set; }
    [JsonPropertyName("genres_total")]
    public int GenresTotal { get; set; }
    [JsonPropertyName("genres_duplicates")]
    public int GenresDuplicates { get; set; }
    [JsonPropertyName("credits_total")]
    public int CreditsTotal { get; set; }
    [JsonPropertyName("credits_duplicates")]
    public int CreditsDuplicates { get; set; }
    [JsonPropertyName("compilation_references_total")]
    public int CompilationReferencesTotal { get; set; }
    [JsonPropertyName("compilation_references_duplicates")]
    public int CompilationReferencesDuplicates { get; set; }
    [JsonPropertyName("promo_art_count")]
    public int PromoArtCount { get; set; }
    [JsonPropertyName("videos_total")]
    public int VideosTotal { get; set; }
    [JsonPropertyName("videos_duplicates")]
    public int VideosDuplicates { get; set; }
}
