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

using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class CompanyLogos
{
    string               _companyName;
    string               _errorMessage;
    bool                  _isLoading = true;
    bool                  _isUploading;
    List<CompanyLogoDto> _logos;
    string               _successMessage;

    [Parameter]
    public int CompanyId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CompanyDto company = await CompaniesService.GetAsync(CompanyId);
        _companyName = company?.Name;
        await LoadLogosAsync();
    }

    async Task LoadLogosAsync()
    {
        _isLoading = true;
        _logos     = await CompanyLogosService.GetByCompany(CompanyId);
        _isLoading = false;
    }

    async Task OnFileSelected(IBrowserFile file)
    {
        _isUploading = true;
        StateHasChanged();

        try
        {
            // Max 5MB for SVG files
            await using var stream      = file.OpenReadStream(5 * 1024 * 1024);
            using var       memoryStream = new System.IO.MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] svgBytes = memoryStream.ToArray();

            (CompanyLogoDto logo, string error) = await CompanyLogosService.UploadAsync(CompanyId, svgBytes, null);

            if(logo is not null)
            {
                _successMessage = "Logo uploaded successfully.";
                await LoadLogosAsync();
            }
            else
            {
                _errorMessage = error ?? "Failed to upload logo.";
            }
        }
        catch(System.Exception ex)
        {
            _errorMessage = $"Upload error: {ex.Message}";
        }
        finally
        {
            _isUploading = false;
            StateHasChanged();
        }
    }

    async Task OpenChangeYearDialog(CompanyLogoDto logo)
    {
        DialogParameters<CompanyLogoYearDialog> parameters = new()
        {
            { x => x.Year, logo.Year }
        };

        IDialogReference dialog = await DialogService.ShowAsync<CompanyLogoYearDialog>("Change Year", parameters,
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.ExtraSmall,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            int? newYear = result.Data as int?;

            (bool succeeded, string error) = await CompanyLogosService.ChangeYearAsync(logo.Id ?? 0, newYear);

            if(succeeded)
            {
                _successMessage = "Year updated.";
                await LoadLogosAsync();
            }
            else
            {
                _errorMessage = error ?? "Failed to update year.";
            }
        }
    }

    async Task ConfirmDeleteLogo(CompanyLogoDto logo)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            { x => x.ContentText, "Are you sure you want to delete this logo? This action cannot be undone." }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>("Delete Logo", parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string error) = await CompanyLogosService.DeleteAsync(logo.Id ?? 0);

            if(succeeded)
            {
                _successMessage = "Logo deleted.";
                await LoadLogosAsync();
            }
            else
            {
                _errorMessage = error ?? "Failed to delete logo.";
            }
        }
    }

    void GoBack() => NavigationManager.NavigateTo("/admin/companies");
}
