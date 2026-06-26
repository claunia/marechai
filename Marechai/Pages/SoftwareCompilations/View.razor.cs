using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.SoftwareCompilations;

public partial class View
{
    SoftwareCompilationDto                        _compilation;
    Guid?                                         _frontCoverId;
    List<SoftwareReleaseDto>                      _releases = [];
    List<SoftwareBySoftwareCompilationDto>        _includedSoftware = [];
    List<SoftwareVersionBySoftwareCompilationDto> _includedVersions = [];
    List<SoftwareCompilationDto>                  _includedCompilations = [];
    bool                                           _loaded;

    [Parameter] public int Id { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        _loaded      = false;
        _compilation = await Service.GetAsync(Id);

        if(_compilation is not null)
        {
            _frontCoverId         = _compilation.FrontCoverId;
            _releases             = await Service.GetReleasesAsync(Id);
            _includedSoftware     = await Service.GetIncludedSoftwareAsync(Id);
            _includedVersions     = await Service.GetIncludedVersionsAsync(Id);
            _includedCompilations = await Service.GetIncludedCompilationsAsync(Id);
        }

        _loaded = true;
    }
}
