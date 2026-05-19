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
using Marechai.ApiClient.Models;

namespace Marechai.Pages.Admin;

public enum VersionMatchType
{
    Exact,
    None
}

public sealed class SoftwareReleaseBulkCreateRow
{
    public string VersionInput   { get; set; }
    public string Title          { get; set; }
    public string PlatformInput  { get; set; }
    public string RegionInput    { get; set; }
    public string LanguageInput  { get; set; }
    public string PublisherInput { get; set; }

    public int? ReleaseYear  { get; set; }
    public int? ReleaseMonth { get; set; }
    public int? ReleaseDay   { get; set; }

    public SoftwareVersionDto MatchedVersion { get; set; }
    public VersionMatchType   VersionMatch   { get; set; } = VersionMatchType.None;

    public List<SoftwarePlatformDto> MatchedPlatforms { get; set; } = [];
    public SoftwarePlatformDto       SelectedPlatform { get; set; }
    public PlatformMatchType         PlatformMatch    { get; set; } = PlatformMatchType.New;

    public List<UnM49Dto>   MatchedRegions { get; set; } = [];
    public UnM49Dto         SelectedRegion { get; set; }
    public CompanyMatchType RegionMatch    { get; set; } = CompanyMatchType.None;

    public List<Iso639Dto>  MatchedLanguages { get; set; } = [];
    public Iso639Dto        SelectedLanguage { get; set; }
    public CompanyMatchType LanguageMatch    { get; set; } = CompanyMatchType.None;

    public List<CompanyDto> MatchedPublishers { get; set; } = [];
    public CompanyDto       SelectedPublisher { get; set; }
    public CompanyMatchType PublisherMatch    { get; set; } = CompanyMatchType.None;

    public string ValidationError { get; set; }
    public string ImportError     { get; set; }

    public bool HasDateWarning => ReleaseYear is null;

    public DateTime? ReleaseDate
    {
        get
        {
            if(ReleaseYear is null)
                return null;

            int month = ReleaseMonth ?? 1;
            int day   = ReleaseDay   ?? 1;

            try
            {
                return new DateTime(ReleaseYear.Value, month, day);
            }
            catch
            {
                return null;
            }
        }
    }

    public int ReleaseDatePrecision
    {
        get
        {
            if(ReleaseYear is null)
                return 0;

            if(ReleaseMonth is null)
                return 2;

            if(ReleaseDay is null)
                return 1;

            return 0;
        }
    }

    public string VersionDisplay
    {
        get
        {
            if(VersionMatch == VersionMatchType.Exact && MatchedVersion is not null)
                return MatchedVersion.VersionString ?? VersionInput ?? "";

            return VersionInput ?? "";
        }
    }

    public string PlatformDisplay
    {
        get
        {
            if(PlatformMatch is PlatformMatchType.Exact or PlatformMatchType.Partial && SelectedPlatform is not null)
                return SelectedPlatform.Name ?? PlatformInput ?? "";

            return PlatformInput ?? "";
        }
    }

    public string RegionDisplay
    {
        get
        {
            if(RegionMatch is CompanyMatchType.Exact or CompanyMatchType.Partial && SelectedRegion is not null)
                return SelectedRegion.Name ?? RegionInput ?? "";

            return RegionInput ?? "";
        }
    }

    public string LanguageDisplay
    {
        get
        {
            if(LanguageMatch is CompanyMatchType.Exact or CompanyMatchType.Partial && SelectedLanguage is not null)
                return SelectedLanguage.ReferenceName ?? LanguageInput ?? "";

            return LanguageInput ?? "";
        }
    }

    public string PublisherDisplay
    {
        get
        {
            if(PublisherMatch is CompanyMatchType.Exact or CompanyMatchType.Partial && SelectedPublisher is not null)
                return SelectedPublisher.Name ?? PublisherInput ?? "";

            return PublisherInput ?? "";
        }
    }
}
