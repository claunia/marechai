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
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Software;

public partial class View
{
    List<SoftwareCompanyRoleDto>                 _companies = [];
    List<SoftwareReleaseDto>                     _compilations = [];
    string?                                      _description;
    int                                         _id;
    bool                                        _loaded;
    List<SoftwareReleaseDto>                     _releases = [];
    List<SoftwareScreenshotDto>                  _screenshots = [];
    Dictionary<string, List<SoftwareScreenshotDto>> _screenshotsByPlatform = new();
    SoftwareScreenshotDto?                       _fullscreenScreenshot;
    List<SoftwareCoverDto>                       _covers = [];
    Dictionary<string, List<SoftwareCoverDto>>    _coversByRelease = new();
    SoftwareCoverDto?                            _fullscreenCover;
    SoftwareDto                                 _software;
    List<SoftwareVersionDto>                    _versions = [];

    [Parameter]
    public int Id
    {
        get => _id;
        set
        {
            if(_id == value) return;

            _id     = value;
            _loaded = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        if(Id <= 0)
        {
            _loaded = true;

            return;
        }

        _software = await Service.GetSoftwareByIdAsync(Id);

        if(_software is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _companies = await Service.GetCompaniesAsync(Id);
        _versions  = await Service.GetVersionsAsync(Id);

        // Load description with language fallback to English
        _description = await Service.GetDescriptionTextAsync(Id);
        _versions.Sort((a, b) => NaturalStringComparer.Instance.Compare(a.VersionString, b.VersionString));

        // Load all non-compilation releases for this software (flat list)
        _releases = await Service.GetReleasesBySoftwareAsync(Id);

        // Load covers from all releases
        _covers = await Service.GetCoversBySoftwareAsync(Id);

        _coversByRelease = _covers
                           .GroupBy(c =>
                            {
                                string label = c.PlatformName ?? "Unknown";

                                if(!string.IsNullOrEmpty(c.RegionNames))
                                    label += " — " + c.RegionNames;

                                return label;
                            })
                           .OrderBy(g => g.Key)
                           .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Type).ToList());

        // Load compilations that include this software
        _compilations = await Service.GetCompilationsForSoftwareAsync(Id);

        // Load screenshots
        List<Guid?> screenshotIds = await Service.GetScreenshotIdsAsync(Id);

        _screenshots = [];

        foreach(Guid? id in screenshotIds)
        {
            if(id.HasValue && id.Value != Guid.Empty)
            {
                SoftwareScreenshotDto? detail = await Service.GetScreenshotDetailsAsync(id.Value);

                if(detail != null) _screenshots.Add(detail);
            }
        }

        // Group screenshots by platform
        _screenshotsByPlatform = _screenshots
                                .GroupBy(s => s.PlatformName ?? "Unknown")
                                .OrderBy(g => g.Key)
                                .ToDictionary(g => g.Key, g => g.ToList());

        _loaded = true;
        StateHasChanged();
    }
}
