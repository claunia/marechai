using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoundSynths
{
    string?              _errorMessage;
    bool                 _isLoading = true;
    string?              _successMessage;
    List<SoundSynthDto>? _soundSynths;

    protected override async Task OnInitializedAsync() => await LoadSoundSynthsAsync();

    async Task LoadSoundSynthsAsync()
    {
        _isLoading   = true;
        _soundSynths = await SoundSynthsService.GetAllAsync();
        _isLoading   = false;
    }

    Func<SoundSynthDto, bool> QuickFilter => _ => true;

    static string FormatDate(DateTimeOffset? date) => date is null ? "" : date.Value.Date.ToShortDateString();

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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoundSynthDialogResult data })
        {
            var dto = new SoundSynthDto
            {
                Name       = data.Name,
                CompanyId  = data.CompanyId,
                ModelCode  = data.ModelCode,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value) : null,
                Voices     = data.Voices,
                Frequency  = data.Frequency,
                Depth      = data.Depth,
                SquareWave = data.SquareWave,
                WhiteNoise = data.WhiteNoise,
                Type       = data.Type
            };

            (long? id, string? errorMessage) = await SoundSynthsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Sound synth created successfully."];
                await LoadSoundSynthsAsync();
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
            { x => x.Introduced, soundSynth.Introduced?.DateTime },
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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoundSynthDialogResult data })
        {
            var dto = new SoundSynthDto
            {
                Id         = soundSynth.Id,
                Name       = data.Name,
                CompanyId  = data.CompanyId,
                ModelCode  = data.ModelCode,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value) : null,
                Voices     = data.Voices,
                Frequency  = data.Frequency,
                Depth      = data.Depth,
                SquareWave = data.SquareWave,
                WhiteNoise = data.WhiteNoise,
                Type       = data.Type
            };

            (bool succeeded, string? errorMessage) = await SoundSynthsService.UpdateAsync(soundSynth.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Sound synth updated successfully."];
                await LoadSoundSynthsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await SoundSynthsService.DeleteAsync(soundSynth.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Sound synth deleted successfully."];
                await LoadSoundSynthsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
