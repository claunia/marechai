using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoundSynths
{
    string                     _errorMessage;
    string                     _successMessage;
    MudDataGrid<SoundSynthDto> _dataGrid;

    async Task<GridData<SoundSynthDto>> ServerReload(GridState<SoundSynthDto> state,
                                                     CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<SoundSynthDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by SoundSynthsController.ApplyFilters. Column comes from
        // PropertyColumn binding via fd.Column?.PropertyName which matches the
        // case labels in the controller switch (Name/Company/ModelCode/Introduced).
        List<string> filters = null;

        foreach(IFilterDefinition<SoundSynthDto> fd in state.FilterDefinitions)
        {
            string column = fd.Column?.PropertyName;
            string op     = fd.Operator;
            if(string.IsNullOrEmpty(column) || string.IsNullOrEmpty(op)) continue;

            bool isEmptyOp = op is "is empty" or "is not empty";

            string value;

            switch(fd.Value)
            {
                case null:
                    if(!isEmptyOp) continue;
                    value = string.Empty;
                    break;
                case DateTime dt:
                    value = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case DateTimeOffset dto:
                    value = dto.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case IFormattable f:
                    value = f.ToString(null, CultureInfo.InvariantCulture);
                    break;
                default:
                    value = fd.Value.ToString();
                    break;
            }

            // Skip non-empty-check operators with no meaningful value so a freshly
            // opened (but unfilled) filter UI doesn't accidentally drop every row.
            if(!isEmptyOp && string.IsNullOrEmpty(value)) continue;

            filters ??= [];
            filters.Add($"{column}||{op}||{value}");
        }

        Task<int>                 countTask = SoundSynthsService.GetCountAsync(filters, cancellationToken);
        Task<List<SoundSynthDto>> dataTask  =
            SoundSynthsService.GetPagedAsync(skip, take, sortBy, sortDescending, filters, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<SoundSynthDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

    async Task OpenAddSoundSynthDialog()
    {
        DialogParameters<SoundSynthDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoundSynthDialog>(L["Add Sound Synth"], parameters,
                                                                                  new DialogOptions
                                                                                  {
                                                                                      MaxWidth  = MaxWidth.Medium,
                                                                                      FullWidth = true
                                                                                  });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoundSynthDialogResult data })
        {
            var dto = new SoundSynthDto
            {
                Name       = data.Name,
                CompanyId  = data.CompanyId,
                ModelCode  = data.ModelCode,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                IntroducedPrecision = data.IntroducedPrecision,
                Voices     = data.Voices,
                Frequency  = data.Frequency,
                Depth      = data.Depth,
                SquareWave = data.SquareWave,
                WhiteNoise = data.WhiteNoise,
                Type       = data.Type
            };

            (long? id, string errorMessage) = await SoundSynthsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Sound synth created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditSoundSynthDialog(SoundSynthDto soundSynth)
    {
        DialogParameters<SoundSynthDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.SoundSynthId, soundSynth.Id ?? 0 },
            { x => x.Name, soundSynth.Name },
            { x => x.CompanyId, soundSynth.CompanyId },
            { x => x.ModelCode, soundSynth.ModelCode },
            { x => x.Introduced, soundSynth.Introduced?.UtcDateTime },
            { x => x.IntroducedPrecision, soundSynth.IntroducedPrecision ?? 0 },
            { x => x.Voices, soundSynth.Voices },
            { x => x.Frequency, soundSynth.Frequency },
            { x => x.Depth, soundSynth.Depth },
            { x => x.SquareWave, soundSynth.SquareWave },
            { x => x.WhiteNoise, soundSynth.WhiteNoise },
            { x => x.Type, soundSynth.Type }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoundSynthDialog>(L["Edit Sound Synth"], parameters,
                                                                                  new DialogOptions
                                                                                  {
                                                                                      MaxWidth  = MaxWidth.Medium,
                                                                                      FullWidth = true
                                                                                  });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoundSynthDialogResult data })
        {
            var dto = new SoundSynthDto
            {
                Id         = soundSynth.Id,
                Name       = data.Name,
                CompanyId  = data.CompanyId,
                ModelCode  = data.ModelCode,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                IntroducedPrecision = data.IntroducedPrecision,
                Voices     = data.Voices,
                Frequency  = data.Frequency,
                Depth      = data.Depth,
                SquareWave = data.SquareWave,
                WhiteNoise = data.WhiteNoise,
                Type       = data.Type
            };

            (bool succeeded, string errorMessage) = await SoundSynthsService.UpdateAsync(soundSynth.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Sound synth updated successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenImportDialog()
    {
        IDialogReference dialog = await DialogService.ShowAsync<SoundSynthImportDialog>(L["Import Sound Synths"],
                                                                                        new DialogOptions
                                                                                        {
                                                                                            MaxWidth  = MaxWidth.ExtraLarge,
                                                                                            FullWidth = true
                                                                                        });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await _dataGrid.ReloadServerData();
    }

    async Task ConfirmDeleteSoundSynth(SoundSynthDto soundSynth)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete sound synth '{0}'? This action cannot be undone."],
                              soundSynth.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Sound Synth"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoundSynthsService.DeleteAsync(soundSynth.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Sound synth deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenDescriptionsDialog(SoundSynthDto soundSynth)
    {
        DialogParameters<SoundSynthDescriptionDialog> parameters = new()
        {
            { x => x.SoundSynthId, soundSynth.Id ?? 0 },
            { x => x.SoundSynthName, soundSynth.Name }
        };

        await DialogService.ShowAsync<SoundSynthDescriptionDialog>(L["Sound Synth Descriptions"], parameters,
                                                                    new DialogOptions
                                                                    {
                                                                        MaxWidth  = MaxWidth.Medium,
                                                                        FullWidth = true
                                                                    });
    }
}
