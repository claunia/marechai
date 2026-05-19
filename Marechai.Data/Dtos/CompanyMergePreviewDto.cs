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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class CompanyMergePreviewDto
{
    [JsonPropertyName("target_id")]
    public int TargetId { get; set; }
    [JsonPropertyName("source_id")]
    public int SourceId { get; set; }
    [JsonPropertyName("target_name")]
    [Required]
    public required string TargetName { get; set; }
    [JsonPropertyName("source_name")]
    [Required]
    public required string SourceName { get; set; }
    [JsonPropertyName("logos_count")]
    public int LogosCount { get; set; }
    [JsonPropertyName("descriptions_total")]
    public int DescriptionsTotal { get; set; }
    [JsonPropertyName("descriptions_duplicates")]
    public int DescriptionsDuplicates { get; set; }
    [JsonPropertyName("gpus_count")]
    public int GpusCount { get; set; }
    [JsonPropertyName("processors_count")]
    public int ProcessorsCount { get; set; }
    [JsonPropertyName("sound_synths_count")]
    public int SoundSynthsCount { get; set; }
    [JsonPropertyName("machines_count")]
    public int MachinesCount { get; set; }
    [JsonPropertyName("machine_families_count")]
    public int MachineFamiliesCount { get; set; }
    [JsonPropertyName("software_releases_count")]
    public int SoftwareReleasesCount { get; set; }
    [JsonPropertyName("software_roles_total")]
    public int SoftwareRolesTotal { get; set; }
    [JsonPropertyName("software_roles_duplicates")]
    public int SoftwareRolesDuplicates { get; set; }
    [JsonPropertyName("people_count")]
    public int PeopleCount { get; set; }
    [JsonPropertyName("books_total")]
    public int BooksTotal { get; set; }
    [JsonPropertyName("books_duplicates")]
    public int BooksDuplicates { get; set; }
    [JsonPropertyName("documents_total")]
    public int DocumentsTotal { get; set; }
    [JsonPropertyName("documents_duplicates")]
    public int DocumentsDuplicates { get; set; }
    [JsonPropertyName("magazines_total")]
    public int MagazinesTotal { get; set; }
    [JsonPropertyName("magazines_duplicates")]
    public int MagazinesDuplicates { get; set; }
    [JsonPropertyName("software_versions_total")]
    public int SoftwareVersionsTotal { get; set; }
    [JsonPropertyName("software_versions_duplicates")]
    public int SoftwareVersionsDuplicates { get; set; }
    [JsonPropertyName("software_families_total")]
    public int SoftwareFamiliesTotal { get; set; }
    [JsonPropertyName("software_families_duplicates")]
    public int SoftwareFamiliesDuplicates { get; set; }
    [JsonPropertyName("inverse_sold_to_count")]
    public int InverseSoldToCount { get; set; }
    [JsonPropertyName("search_entry_company_ref_count")]
    public int SearchEntryCompanyRefCount { get; set; }
}
