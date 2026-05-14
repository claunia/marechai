using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class People
{
    string          _errorMessage;
    bool             _isLoading = true;
    string          _successMessage;
    List<PersonDto> _people;

    protected override async Task OnInitializedAsync() => await LoadPeopleAsync();

    async Task LoadPeopleAsync()
    {
        _isLoading = true;
        _people    = await PeopleService.GetPeopleAsync();
        _isLoading = false;
    }

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

    async Task OpenAddPersonDialog()
    {
        DialogParameters<PersonDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<PersonDialog>(L["Add Person"], parameters,
                                                                              new DialogOptions
                                                                              {
                                                                                  MaxWidth  = MaxWidth.Medium,
                                                                                  FullWidth = true
                                                                              });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: PersonDialogResult data })
        {
            var dto = new PersonDto
            {
                Name        = data.Name,
                Surname     = data.Surname,
                Alias       = data.Alias,
                DisplayName = data.DisplayName,
                CountryId   = data.CountryId,
                Birthdate           = data.BirthDate.HasValue ? new DateTimeOffset(data.BirthDate.Value, TimeSpan.Zero) : null,
                BirthdatePrecision  = data.BirthDatePrecision,
                DeathDate           = data.DeathDate.HasValue ? new DateTimeOffset(data.DeathDate.Value, TimeSpan.Zero) : null,
                DeathDatePrecision  = data.DeathDatePrecision,
                Webpage     = data.Webpage,
                Twitter     = data.Twitter,
                Facebook    = data.Facebook
            };

            (long? id, string errorMessage) = await PeopleService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Person created successfully."];
                await LoadPeopleAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditPersonDialog(PersonDto person)
    {
        // Fetch full details by ID to get FK IDs (list endpoint may omit them)
        PersonDto fullPerson = person.Id.HasValue ? await PeopleService.GetPersonAsync(person.Id.Value) : person;
        fullPerson ??= person;

        DialogParameters<PersonDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.PersonId, fullPerson.Id ?? 0 },
            { x => x.Name, fullPerson.Name },
            { x => x.Surname, fullPerson.Surname },
            { x => x.Alias, fullPerson.Alias },
            { x => x.DisplayName, fullPerson.DisplayName },
            { x => x.CountryId, fullPerson.CountryId },
            { x => x.BirthDate, fullPerson.Birthdate?.UtcDateTime },
            { x => x.BirthDatePrecision, fullPerson.BirthdatePrecision ?? 0 },
            { x => x.DeathDate, fullPerson.DeathDate?.UtcDateTime },
            { x => x.DeathDatePrecision, fullPerson.DeathDatePrecision ?? 0 },
            { x => x.Webpage, fullPerson.Webpage },
            { x => x.Twitter, fullPerson.Twitter },
            { x => x.Facebook, fullPerson.Facebook },
            { x => x.HasPhoto, fullPerson.Photo.HasValue && fullPerson.Photo.Value != Guid.Empty }
        };

        IDialogReference dialog = await DialogService.ShowAsync<PersonDialog>(L["Edit Person"], parameters,
                                                                              new DialogOptions
                                                                              {
                                                                                  MaxWidth  = MaxWidth.Medium,
                                                                                  FullWidth = true
                                                                              });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: PersonDialogResult data })
        {
            var dto = new PersonDto
            {
                Id          = person.Id,
                Name        = data.Name,
                Surname     = data.Surname,
                Alias       = data.Alias,
                DisplayName = data.DisplayName,
                CountryId   = data.CountryId,
                Birthdate           = data.BirthDate.HasValue ? new DateTimeOffset(data.BirthDate.Value, TimeSpan.Zero) : null,
                BirthdatePrecision  = data.BirthDatePrecision,
                DeathDate           = data.DeathDate.HasValue ? new DateTimeOffset(data.DeathDate.Value, TimeSpan.Zero) : null,
                DeathDatePrecision  = data.DeathDatePrecision,
                Webpage     = data.Webpage,
                Twitter     = data.Twitter,
                Facebook    = data.Facebook
            };

            (bool succeeded, string errorMessage) = await PeopleService.UpdateAsync(person.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Person updated successfully."];
                await LoadPeopleAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDeletePerson(PersonDto person)
    {
        string displayName = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete person '{0}'? This action cannot be undone."],
                              displayName)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Person"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await PeopleService.DeleteAsync(person.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Person deleted successfully."];
                await LoadPeopleAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenImportDialog()
    {
        IDialogReference dialog = await DialogService.ShowAsync<PersonImportDialog>(L["Import CSV"],
                                                                                    new DialogOptions
                                                                                    {
                                                                                        MaxWidth  = MaxWidth.ExtraLarge,
                                                                                        FullWidth = true
                                                                                    });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await LoadPeopleAsync();
    }

    async Task OpenDescriptionsDialog(PersonDto person)
    {
        string displayName = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}".Trim();

        DialogParameters<PersonDescriptionDialog> parameters = new()
        {
            { x => x.PersonId, person.Id ?? 0 },
            { x => x.PersonName, displayName }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<PersonDescriptionDialog>(L["Person Descriptions"], parameters,
                                                                   new DialogOptions
                                                                   {
                                                                       MaxWidth  = MaxWidth.Medium,
                                                                       FullWidth = true
                                                                   });

        await dialog.Result;
    }
}
