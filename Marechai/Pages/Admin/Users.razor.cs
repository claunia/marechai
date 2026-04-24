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
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Users
{
    string?        _errorMessage;
    bool           _isLoading = true;
    string?        _successMessage;
    List<UserDto>? _users;

    protected override async Task OnInitializedAsync() => await LoadUsersAsync();

    async Task LoadUsersAsync()
    {
        _isLoading = true;
        _users     = await UsersService.GetAllAsync();
        _isLoading = false;
    }

    async Task OpenAddUserDialog()
    {
        DialogParameters<UserDialog> parameters = new()
        {
            {
                x => x.IsNew, true
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<UserDialog>("Add User", parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth = MaxWidth.Small,
                                                                                FullWidth = true
                                                                            });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: UserDialogResult data })
        {
            (bool succeeded, string? errorMessage) =
                await UsersService.CreateAsync(data.Email, data.UserName, data.Password!, data.PhoneNumber);

            if(succeeded)
            {
                _successMessage = "User created successfully.";
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditUserDialog(UserDto user)
    {
        DialogParameters<UserDialog> parameters = new()
        {
            {
                x => x.IsNew, false
            },
            {
                x => x.Email, user.Email
            },
            {
                x => x.UserName, user.UserName
            },
            {
                x => x.PhoneNumber, user.PhoneNumber
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<UserDialog>("Edit User", parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth = MaxWidth.Small,
                                                                                FullWidth = true
                                                                            });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: UserDialogResult data })
        {
            (bool succeeded, string? errorMessage) =
                await UsersService.UpdateAsync(user.Id, data.Email, data.UserName, data.PhoneNumber);

            if(succeeded)
            {
                _successMessage = "User updated successfully.";
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenChangePasswordDialog(UserDto user)
    {
        DialogParameters<PasswordDialog> parameters = new()
        {
            {
                x => x.UserEmail, user.Email
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<PasswordDialog>("Change Password", parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth = MaxWidth.Small,
                                                                                    FullWidth = true
                                                                                });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: string newPassword })
        {
            (bool succeeded, string? errorMessage) = await UsersService.ChangePasswordAsync(user.Id, newPassword);

            if(succeeded)
            {
                _successMessage = "Password changed successfully.";
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenManageRolesDialog(UserDto user)
    {
        List<string> availableRoles = await UsersService.GetRolesAsync();

        DialogParameters<RolesDialog> parameters = new()
        {
            {
                x => x.UserId, user.Id
            },
            {
                x => x.UserEmail, user.Email
            },
            {
                x => x.CurrentRoles, user.Roles?.ToList() ?? []
            },
            {
                x => x.AvailableRoles, availableRoles
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<RolesDialog>("Manage Roles", parameters,
                                                                             new DialogOptions
                                                                             {
                                                                                 MaxWidth = MaxWidth.Small,
                                                                                 FullWidth = true
                                                                             });

        await dialog.Result;

        // Always reload since roles may have changed
        await LoadUsersAsync();
    }

    async Task ConfirmDeleteUser(UserDto user)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText, $"Are you sure you want to delete user '{user.Email}'? This action cannot be undone."
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>("Delete User", parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await UsersService.DeleteAsync(user.Id);

            if(succeeded)
            {
                _successMessage = "User deleted successfully.";
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
