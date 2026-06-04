using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class People
{
    string                 _errorMessage;
    string                 _searchText;
    string                 _successMessage;
    MudDataGrid<PersonDto> _dataGrid;

    async Task<GridData<PersonDto>> ServerReload(GridState<PersonDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<PersonDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by PeopleController.ApplyFilters. Column comes from the
        // PropertyColumn's PropertyName which matches the switch labels in the
        // controller (Name/Surname/Country/Birthdate/DeathDate).
        List<string> filters = null;

        foreach(IFilterDefinition<PersonDto> fd in state.FilterDefinitions)
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

        Task<int>             countTask = PeopleService.GetPeopleCountAsync(filters, _searchText, cancellationToken);
        Task<List<PersonDto>> dataTask  = PeopleService.GetPeopleAsync(skip, take, sortBy, sortDescending, filters,
                                                                       _searchText, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<PersonDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    async Task OnSearch(string text)
    {
        _searchText = text;
        await _dataGrid.ReloadServerData();
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
                await _dataGrid.ReloadServerData();
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
                await _dataGrid.ReloadServerData();
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
        IDialogReference dialog = await DialogService.ShowAsync<PersonImportDialog>(L["Import CSV"],
                                                                                    new DialogOptions
                                                                                    {
                                                                                        MaxWidth  = MaxWidth.ExtraLarge,
                                                                                        FullWidth = true
                                                                                    });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await _dataGrid.ReloadServerData();
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
