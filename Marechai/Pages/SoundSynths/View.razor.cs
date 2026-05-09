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
using Marechai.Helpers;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.SoundSynths;

public partial class View
{
    List<MachineDto> _computers = [];
    List<MachineDto> _consoles  = [];
    List<MachineDto> _smartphones = [];
    string          _description;
    string           _displayName;
    int              _lastId;
    bool             _loaded;
    List<Guid>       _photos = [];
    PhotoLightbox    _lightbox;
    SoundSynthDto    _synth;
    List<SoundSynthVideoDto> _videos = [];

    // Tab state — backs the responsive sticky MudTabs in View.razor.
    // _tabNames is rebuilt after data loads to include "machines" only when
    // at least one of the per-machine-type lists is non-empty.
    string[] _tabNames  = ["specifications", "media"];
    string   _activeTab = "specifications";

    int _activeTabIndex => Math.Max(0, Array.IndexOf(_tabNames, _activeTab));

    // Sentinel rows (currently only DB_SOFTWARE, id = -2 per Marechai.Database
    // Operations.DbSoftware) represent abstract/virtual sound synthesizers and
    // have no specifications, machines, photos, or videos. Detect by name so
    // the rename pattern matches the existing _displayName line below.
    bool _isSentinel => _synth?.Name == "DB_SOFTWARE";

    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromQuery(Name = "tab")]
    public string TabParam { get; set; }

    [Inject]
    NavigationManager NavManager { get; set; }

    protected override void OnParametersSet()
    {
        // Validate against the static superset; the index resolver collapses
        // missing tabs (e.g. "machines" on a synth with none) to 0.
        string tab = (TabParam ?? "specifications").ToLowerInvariant();
        _activeTab = tab is "specifications" or "machines" or "media" ? tab : "specifications";

        if(Id == _lastId) return;

        _lastId = Id;
        _loaded = false;
    }

    void OnTabChanged(int index)
    {
        if(index < 0 || index >= _tabNames.Length) return;

        string newTab = _tabNames[index];

        if(newTab == _activeTab) return;

        _activeTab = newTab;

        // Default tab (specifications) drops the query param to keep URLs clean.
        string newUri = NavManager.GetUriWithQueryParameter("tab", newTab == "specifications" ? null : newTab);
        NavManager.NavigateTo(newUri, false, true);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        // ── Phase 1+3 retrofit ────────────────────────────────────────────────
        // One consolidated HTTP round-trip via /sound-synths/{Id}/full (head
        // projection + company logo via inline subquery + description with
        // single-query language fallback + machines + photos + videos), all
        // executed in parallel server-side on independent DbContext instances.
        // Replaces the original 5 sequential GetByIdAsync /
        // GetMachinesBySoundSynthAsync / GetDescriptionTextAsync /
        // GetGuidsBySoundSynthAsync / GetVideosBySoundSynthAsync calls.
        SoundSynthFullDto full = await Service.GetFullAsync(Id, UiLanguage.GetIso639_3());

        if(full?.SoundSynth is null)
        {
            _synth  = null;
            _loaded = true;
            StateHasChanged();

            return;
        }

        _synth       = full.SoundSynth;
        _displayName = _synth.Name == "DB_SOFTWARE" ? L["Software"] : _synth.Name;

        // Photos arrive as List<Guid?>? (Kiota emits nullable element type even
        // for non-nullable server collections). Materialise into the existing
        // List<Guid> field shape that the .razor view expects.
        _photos = full.Photos?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];

        _videos = full.Videos ?? [];

        _description = full.DescriptionHtml ?? full.DescriptionText;

        List<MachineDto> machines = full.Machines ?? [];
        _computers   = machines.Where(m => m.Type == (int)MachineType.Computer).ToList();
        _consoles    = machines.Where(m => m.Type == (int)MachineType.Console).ToList();
        _smartphones = machines.Where(m => m.Type == (int)MachineType.Smartphone).ToList();

        // Insert the Machines tab between Specifications and Media when the
        // synth has any attached computers/consoles/smartphones, so
        // _activeTabIndex resolves "machines" correctly on first paint after a
        // deep link. Sentinel rows hide the entire tab block at the markup
        // level, so leave _tabNames at its default for them.
        if(!_isSentinel)
        {
            bool hasMachines = _computers.Count > 0 || _consoles.Count > 0 || _smartphones.Count > 0;
            _tabNames = hasMachines
                            ? ["specifications", "machines", "media"]
                            : ["specifications", "media"];
        }

        _loaded = true;
        StateHasChanged();
    }
}
