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
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Companies
{
    string                    _errorMessage;
    string                    _successMessage;
    string                    _searchText;
    MudDataGrid<CompanyDto>   _dataGrid;

    async Task<GridData<CompanyDto>> ServerReload(GridState<CompanyDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<CompanyDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        Task<int>              countTask = CompaniesService.GetCountAsync(_searchText);
        Task<List<CompanyDto>> dataTask  = CompaniesService.GetPagedAsync(skip, take, _searchText, sortBy, sortDescending);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<CompanyDto>
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

    string GetStatusText(int? status) => status switch
    {
        0 => L["Unknown"],
        1 => L["Active"],
        2 => L["Sold"],
        3 => L["Merged"],
        4 => L["Bankrupt"],
        5 => L["Defunct"],
        6 => L["Renamed"],
        _ => L["Unknown"]
    };

    static Color GetStatusColor(int? status) => status switch
    {
        1 => Color.Success,
        2 => Color.Warning,
        3 => Color.Info,
        4 => Color.Error,
        5 => Color.Dark,
        6 => Color.Secondary,
        _ => Color.Default
    };

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

    async Task OpenAddCompanyDialog()
    {
        DialogParameters<CompanyDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<CompanyDialog>(L["Add Company"], parameters,
                                                                               new DialogOptions
                                                                               {
                                                                                   MaxWidth  = MaxWidth.Medium,
                                                                                   FullWidth = true
                                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: CompanyDialogResult data })
        {
            var dto = new CompanyDto
            {
                Name                  = data.Name,
                LegalName             = data.LegalName,
                Status                = data.Status,
                Founded               = data.Founded.HasValue ? new DateTimeOffset(data.Founded.Value, TimeSpan.Zero) : null,
                FoundedPrecision      = data.FoundedPrecision,
                
                Sold                  = data.Sold.HasValue ? new DateTimeOffset(data.Sold.Value, TimeSpan.Zero) : null,
                SoldPrecision         = data.SoldPrecision,
                
                SoldToId              = data.SoldToId,
                CountryId             = data.CountryId,
                Address               = data.Address,
                City                  = data.City,
                Province              = data.Province,
                PostalCode            = data.PostalCode,
                Website               = data.Website,
                Twitter               = data.Twitter,
                Facebook              = data.Facebook
            };

            (int? id, string errorMessage) = await CompaniesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Company created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditCompanyDialog(CompanyDto company)
    {
        DialogParameters<CompanyDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.CompanyId, company.Id ?? 0 },
            { x => x.Name, company.Name },
            { x => x.LegalName, company.LegalName },
            { x => x.StatusValue, company.Status ?? 0 },
            { x => x.Founded, company.Founded?.UtcDateTime },
            { x => x.FoundedPrecision, company.FoundedPrecision ?? 0 },
            
            { x => x.Sold, company.Sold?.UtcDateTime },
            { x => x.SoldPrecision, company.SoldPrecision ?? 0 },
            
            { x => x.SoldToId, company.SoldToId },
            { x => x.CountryId, company.CountryId },
            { x => x.Address, company.Address },
            { x => x.City, company.City },
            { x => x.Province, company.Province },
            { x => x.PostalCode, company.PostalCode },
            { x => x.Website, company.Website },
            { x => x.Twitter, company.Twitter },
            { x => x.Facebook, company.Facebook }
        };

        IDialogReference dialog = await DialogService.ShowAsync<CompanyDialog>(L["Edit Company"], parameters,
                                                                               new DialogOptions
                                                                               {
                                                                                   MaxWidth  = MaxWidth.Medium,
                                                                                   FullWidth = true
                                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: CompanyDialogResult data })
        {
            var dto = new CompanyDto
            {
                Id                    = company.Id,
                Name                  = data.Name,
                LegalName             = data.LegalName,
                Status                = data.Status,
                Founded               = data.Founded.HasValue ? new DateTimeOffset(data.Founded.Value, TimeSpan.Zero) : null,
                FoundedPrecision      = data.FoundedPrecision,
                
                Sold                  = data.Sold.HasValue ? new DateTimeOffset(data.Sold.Value, TimeSpan.Zero) : null,
                SoldPrecision         = data.SoldPrecision,
                
                SoldToId              = data.SoldToId,
                CountryId             = data.CountryId,
                Address               = data.Address,
                City                  = data.City,
                Province              = data.Province,
                PostalCode            = data.PostalCode,
                Website               = data.Website,
                Twitter               = data.Twitter,
                Facebook              = data.Facebook
            };

            (bool succeeded, string errorMessage) = await CompaniesService.UpdateAsync(company.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Company updated successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDeleteCompany(CompanyDto company)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete company '{0}'? This action cannot be undone."],
                              company.Name)
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete company"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await CompaniesService.DeleteAsync(company.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Company deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenDescriptionsDialog(CompanyDto company)
    {
        DialogParameters<CompanyDescriptionDialog> parameters = new()
        {
            { x => x.CompanyId, company.Id ?? 0 },
            { x => x.CompanyName, company.Name }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<CompanyDescriptionDialog>(L["Company Descriptions"], parameters,
                                                                    new DialogOptions
                                                                    {
                                                                        MaxWidth  = MaxWidth.Medium,
                                                                        FullWidth = true
                                                                    });

        await dialog.Result;
    }

    void NavigateToLogos(CompanyDto company) => NavigationManager.NavigateTo($"/admin/companies/{company.Id ?? 0}/logos");
}
