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
    string              _errorMessage;
    bool                 _isLoading = true;
    HashSet<UserDto>     _selectedUsers = new();
    string              _successMessage;
    List<UserDto>       _users;

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

        IDialogReference dialog = await DialogService.ShowAsync<UserDialog>(L["Add User"], parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth = MaxWidth.Small,
                                                                                FullWidth = true
                                                                            });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: UserDialogResult data })
        {
            (bool succeeded, string errorMessage) =
                await UsersService.CreateAsync(data.Email, data.UserName, data.Password!, data.PhoneNumber);

            if(succeeded)
            {
                _successMessage = L["User created successfully."];
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

        IDialogReference dialog = await DialogService.ShowAsync<UserDialog>(L["Edit User"], parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth = MaxWidth.Small,
                                                                                FullWidth = true
                                                                            });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: UserDialogResult data })
        {
            (bool succeeded, string errorMessage) =
                await UsersService.UpdateAsync(user.Id, data.Email, data.UserName, data.PhoneNumber);

            if(succeeded)
            {
                _successMessage = L["User updated successfully."];
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

        IDialogReference dialog = await DialogService.ShowAsync<PasswordDialog>(L["Change Password"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth = MaxWidth.Small,
                                                                                    FullWidth = true
                                                                                });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: string newPassword })
        {
            (bool succeeded, string errorMessage) = await UsersService.ChangePasswordAsync(user.Id, newPassword);

            if(succeeded)
            {
                _successMessage = L["Password changed successfully."];
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

        IDialogReference dialog = await DialogService.ShowAsync<RolesDialog>(L["Manage Roles"], parameters,
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
                x => x.ContentText, string.Format(L["Are you sure you want to delete user '{0}'? This action cannot be undone."], user.Email)
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete User"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await UsersService.DeleteAsync(user.Id);

            if(succeeded)
            {
                _successMessage = L["User deleted successfully."];
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDisableTwoFactor(UserDto user)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to disable two-factor authentication for user '{0}'? Their authenticator key and recovery codes will be cleared."],
                              user.Email)
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>(L["Disable 2FA"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool ok, string err) = await UsersService.DisableTwoFactorAsync(user.Id);

            if(ok)
            {
                _successMessage = L["2FA disabled for user."];
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = err;
            }
        }
    }

    async Task ConfirmBulkDeleteUsers()
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete {0} user(s)? This action cannot be undone."], _selectedUsers.Count)
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Users"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            List<string> userIds = _selectedUsers.Select(u => u.Id).ToList();

            (ApiClient.Models.BulkOperationResult bulkResult, string errorMessage) =
                await UsersService.BulkDeleteAsync(userIds);

            if(bulkResult != null)
            {
                _successMessage =
                    string.Format(L["Deleted {0} user(s)."], bulkResult.SucceededCount) +
                    (bulkResult.FailedCount > 0 ? " " + string.Format(L["{0} failed."], bulkResult.FailedCount) : "");

                if(bulkResult.Errors is { Count: > 0 })
                    _errorMessage = string.Join(" ", bulkResult.Errors);

                _selectedUsers.Clear();
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenBulkRoleDialog()
    {
        List<string> availableRoles = await UsersService.GetRolesAsync();

        DialogParameters<BulkRoleDialog> parameters = new()
        {
            {
                x => x.UserCount, _selectedUsers.Count
            },
            {
                x => x.AvailableRoles, availableRoles
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<BulkRoleDialog>(L["Bulk Role Operation"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth = MaxWidth.Small,
                                                                                    FullWidth = true
                                                                                });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: BulkRoleDialogResult data })
        {
            List<string> userIds = _selectedUsers.Select(u => u.Id).ToList();

            (ApiClient.Models.BulkOperationResult bulkResult, string errorMessage) = data.IsAdd
                ? await UsersService.BulkAddRoleAsync(userIds, data.RoleName)
                : await UsersService.BulkRemoveRoleAsync(userIds, data.RoleName);

            if(bulkResult != null)
            {
                _successMessage = data.IsAdd
                    ? string.Format(L["Role '{0}' added to {1} user(s)."], data.RoleName, bulkResult.SucceededCount)
                    : string.Format(L["Role '{0}' removed from {1} user(s)."], data.RoleName, bulkResult.SucceededCount);

                if(bulkResult.FailedCount > 0)
                    _successMessage += " " + string.Format(L["{0} failed."], bulkResult.FailedCount);

                if(bulkResult.Errors is { Count: > 0 })
                    _errorMessage = string.Join(" ", bulkResult.Errors);

                _selectedUsers.Clear();
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenBulkLockoutDialog()
    {
        DialogParameters<BulkLockoutDialog> parameters = new()
        {
            {
                x => x.UserCount, _selectedUsers.Count
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<BulkLockoutDialog>(L["Bulk Lockout"], parameters,
                                                                                   new DialogOptions
                                                                                   {
                                                                                       MaxWidth = MaxWidth.ExtraSmall,
                                                                                       FullWidth = true
                                                                                   });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: bool enable })
        {
            List<string> userIds = _selectedUsers.Select(u => u.Id).ToList();

            (ApiClient.Models.BulkOperationResult bulkResult, string errorMessage) =
                await UsersService.BulkSetLockoutAsync(userIds, enable);

            if(bulkResult != null)
            {
                _successMessage = enable
                    ? string.Format(L["Lockout enabled for {0} user(s)."], bulkResult.SucceededCount)
                    : string.Format(L["Lockout disabled for {0} user(s)."], bulkResult.SucceededCount);

                if(bulkResult.FailedCount > 0)
                    _successMessage += " " + string.Format(L["{0} failed."], bulkResult.FailedCount);

                if(bulkResult.Errors is { Count: > 0 })
                    _errorMessage = string.Join(" ", bulkResult.Errors);

                _selectedUsers.Clear();
                await LoadUsersAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
