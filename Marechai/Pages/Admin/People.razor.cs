using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class People
{
    string?          _errorMessage;
    bool             _isLoading = true;
    string?          _successMessage;
    List<PersonDto>? _people;

    protected override async Task OnInitializedAsync() => await LoadPeopleAsync();

    async Task LoadPeopleAsync()
    {
        _isLoading = true;
        _people    = await PeopleService.GetPeopleAsync();
        _isLoading = false;
    }

    Func<PersonDto, bool> QuickFilter => _ => true;

    static string FormatDate(DateTimeOffset? date) => date is null ? "" : date.Value.Date.ToShortDateString();

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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: PersonDialogResult data })
        {
            var dto = new PersonDto
            {
                Name        = data.Name,
                Surname     = data.Surname,
                Alias       = data.Alias,
                DisplayName = data.DisplayName,
                CountryId   = data.CountryId,
                Birthdate   = data.BirthDate.HasValue ? new DateTimeOffset(data.BirthDate.Value) : null,
                DeathDate   = data.DeathDate.HasValue ? new DateTimeOffset(data.DeathDate.Value) : null,
                Webpage     = data.Webpage,
                Twitter     = data.Twitter,
                Facebook    = data.Facebook
            };

            (long? id, string? errorMessage) = await PeopleService.CreateAsync(dto);

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
        PersonDto? fullPerson = person.Id.HasValue ? await PeopleService.GetPersonAsync(person.Id.Value) : person;
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
            { x => x.BirthDate, fullPerson.Birthdate?.DateTime },
            { x => x.DeathDate, fullPerson.DeathDate?.DateTime },
            { x => x.Webpage, fullPerson.Webpage },
            { x => x.Twitter, fullPerson.Twitter },
            { x => x.Facebook, fullPerson.Facebook }
        };

        IDialogReference dialog = await DialogService.ShowAsync<PersonDialog>(L["Edit Person"], parameters,
                                                                              new DialogOptions
                                                                              {
                                                                                  MaxWidth  = MaxWidth.Medium,
                                                                                  FullWidth = true
                                                                              });

        DialogResult? result = await dialog.Result;

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
                Birthdate   = data.BirthDate.HasValue ? new DateTimeOffset(data.BirthDate.Value) : null,
                DeathDate   = data.DeathDate.HasValue ? new DateTimeOffset(data.DeathDate.Value) : null,
                Webpage     = data.Webpage,
                Twitter     = data.Twitter,
                Facebook    = data.Facebook
            };

            (bool succeeded, string? errorMessage) = await PeopleService.UpdateAsync(person.Id ?? 0, dto);

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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await PeopleService.DeleteAsync(person.Id ?? 0);

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
}
