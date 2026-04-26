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

public enum SoftwareMatchType
{
    Exact,
    Partial,
    New
}

public enum PlatformMatchType
{
    Exact,
    Partial,
    New
}

public sealed class SoftwareReleaseImportRow
{
    public string? Title              { get; set; }
    public string? IsCompilationInput { get; set; }
    public string? SoftwareInput      { get; set; }
    public string? PlatformInput      { get; set; }
    public string? PublisherInput     { get; set; }
    public int?    ReleaseYear        { get; set; }
    public int?    ReleaseMonth       { get; set; }
    public int?    ReleaseDay         { get; set; }

    public bool? IsCompilation { get; set; }

    public List<CompanyDto>          MatchedPublishers { get; set; } = [];
    public CompanyDto?               SelectedPublisher { get; set; }
    public CompanyMatchType          PublisherMatch    { get; set; } = CompanyMatchType.None;

    public List<SoftwareDto>         MatchedSoftware   { get; set; } = [];
    public SoftwareDto?              SelectedSoftware  { get; set; }
    public SoftwareMatchType         SoftwareMatch     { get; set; } = SoftwareMatchType.New;

    public List<SoftwarePlatformDto> MatchedPlatforms  { get; set; } = [];
    public SoftwarePlatformDto?      SelectedPlatform  { get; set; }
    public PlatformMatchType         PlatformMatch     { get; set; } = PlatformMatchType.New;

    public bool    IsDuplicate      { get; set; }
    public string? ImportError      { get; set; }
    public string? ValidationError  { get; set; }

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

    public string PublisherDisplay
    {
        get
        {
            if(PublisherMatch is CompanyMatchType.Exact or CompanyMatchType.Partial && SelectedPublisher is not null)
                return SelectedPublisher.Name ?? PublisherInput ?? "";

            return PublisherInput ?? "";
        }
    }

    public string SoftwareDisplay
    {
        get
        {
            if(SoftwareMatch is SoftwareMatchType.Exact or SoftwareMatchType.Partial && SelectedSoftware is not null)
                return SelectedSoftware.Name ?? SoftwareInput ?? "";

            return SoftwareInput ?? "";
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

    public static bool? ParseBoolean(string? input)
    {
        if(string.IsNullOrWhiteSpace(input))
            return null;

        string trimmed = input.Trim();

        if(string.Equals(trimmed, "y",     StringComparison.OrdinalIgnoreCase) ||
           string.Equals(trimmed, "yes",   StringComparison.OrdinalIgnoreCase) ||
           string.Equals(trimmed, "true",  StringComparison.OrdinalIgnoreCase) ||
           string.Equals(trimmed, "1",     StringComparison.OrdinalIgnoreCase))
            return true;

        if(string.Equals(trimmed, "n",     StringComparison.OrdinalIgnoreCase) ||
           string.Equals(trimmed, "no",    StringComparison.OrdinalIgnoreCase) ||
           string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(trimmed, "0",     StringComparison.OrdinalIgnoreCase))
            return false;

        return null;
    }
}
