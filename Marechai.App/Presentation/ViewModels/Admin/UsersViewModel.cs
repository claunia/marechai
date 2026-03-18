#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

/// <summary>
///     ViewModel for user management page (Uberadmin only)
/// </summary>
public partial class UsersViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient               _apiClient;
    private readonly IJwtService             _jwtService;
    private readonly IStringLocalizer        _localizer;
    private readonly ILogger<UsersViewModel> _logger;
    private readonly ITokenService           _tokenService;

    [ObservableProperty]
    private ObservableCollection<string> _availableRoles = [];

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _dialogTitle = string.Empty;

    [ObservableProperty]
    private string _dialogType = string.Empty;

    private string? _editingUserId;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string? _selectedRole;

    [ObservableProperty]
    private bool _isUberadmin;

    [ObservableProperty]
    private UserDto? _selectedUser;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _userRoles = [];

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = [];

    public UsersViewModel(ApiClient               apiClient, IJwtService      jwtService, ITokenService tokenService,
                          ILogger<UsersViewModel> logger,    IStringLocalizer localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadUsersCommand                = new AsyncRelayCommand(LoadUsersAsync);
        DeleteUserCommand               = new AsyncRelayCommand<UserDto>(DeleteUserAsync);
        OpenAddUserDialogCommand        = new AsyncRelayCommand(OpenAddUserDialogAsync);
        OpenEditUserDialogCommand       = new AsyncRelayCommand<UserDto>(OpenEditUserDialogAsync);
        OpenChangePasswordDialogCommand = new AsyncRelayCommand<UserDto>(OpenChangePasswordDialogAsync);
        OpenManageRolesDialogCommand    = new AsyncRelayCommand<UserDto>(OpenManageRolesDialogAsync);
        SaveUserCommand                 = new AsyncRelayCommand(SaveUserAsync);
        SavePasswordCommand             = new AsyncRelayCommand(SavePasswordAsync);
        AddRoleCommand                  = new AsyncRelayCommand(AddRoleAsync);
        RemoveRoleCommand               = new AsyncRelayCommand<string>(RemoveRoleAsync);
        CloseDialogCommand              = new RelayCommand(CloseDialog);

        // Check role immediately
        CheckUberadminRole();
    }

    private void CheckUberadminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsUberadmin = false;

                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);
            IsUberadmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsUberadmin = false;
        }
    }

    public IAsyncRelayCommand          LoadUsersCommand                { get; }
    public IAsyncRelayCommand<UserDto> DeleteUserCommand               { get; }
    public IAsyncRelayCommand          OpenAddUserDialogCommand        { get; }
    public IAsyncRelayCommand<UserDto> OpenEditUserDialogCommand       { get; }
    public IAsyncRelayCommand<UserDto> OpenChangePasswordDialogCommand { get; }
    public IAsyncRelayCommand<UserDto> OpenManageRolesDialogCommand    { get; }
    public IAsyncRelayCommand          SaveUserCommand                 { get; }
    public IAsyncRelayCommand          SavePasswordCommand             { get; }
    public IAsyncRelayCommand          AddRoleCommand                  { get; }
    public IAsyncRelayCommand<string>  RemoveRoleCommand               { get; }
    public IRelayCommand               CloseDialogCommand              { get; }

    public event EventHandler<string>? ShowDialogRequested;

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        // Re-check role when navigated to — ensures binding updates after DataContext is set
        CheckUberadminRole();

        if(IsUberadmin)
            _ = LoadUsersCommand.ExecuteAsync(null);
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Users.Clear();

            List<UserDto>? usersResponse = await _apiClient.Users.GetAsync();

            if(usersResponse != null)
            {
                foreach(UserDto user in usersResponse) Users.Add(user);

                IsDataLoaded = true;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            ErrorMessage = _localizer["Failed to load users. Please try again."];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteUserAsync(UserDto? user)
    {
        if(user?.Id == null) return;

        try
        {
            await _apiClient.Users[user.Id].DeleteAsync();
            await LoadUsersAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            ErrorMessage = _localizer["Failed to delete user."];
            HasError     = true;
        }
    }

    private async Task OpenAddUserDialogAsync()
    {
        try
        {
            _editingUserId  = null;
            DialogTitle     = _localizer["Add User"];
            DialogType      = "AddEdit";
            Email           = string.Empty;
            UserName        = string.Empty;
            PhoneNumber     = string.Empty;
            Password        = string.Empty;
            ConfirmPassword = string.Empty;
            ShowDialogRequested?.Invoke(this, "AddEdit");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error opening add user dialog");
        }
    }

    private async Task OpenEditUserDialogAsync(UserDto? user)
    {
        if(user?.Id == null) return;

        try
        {
            _editingUserId  = user.Id;
            DialogTitle     = _localizer["Edit User"];
            DialogType      = "AddEdit";
            Email           = user.Email       ?? string.Empty;
            UserName        = user.UserName    ?? string.Empty;
            PhoneNumber     = user.PhoneNumber ?? string.Empty;
            Password        = string.Empty;
            ConfirmPassword = string.Empty;
            ShowDialogRequested?.Invoke(this, "AddEdit");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error opening edit user dialog");
        }
    }

    private async Task OpenChangePasswordDialogAsync(UserDto? user)
    {
        if(user?.Id == null) return;

        try
        {
            _editingUserId  = user.Id;
            DialogTitle     = _localizer["Change Password"];
            DialogType      = "Password";
            Password        = string.Empty;
            ConfirmPassword = string.Empty;
            ShowDialogRequested?.Invoke(this, "Password");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error opening change password dialog");
        }
    }

    private async Task OpenManageRolesDialogAsync(UserDto? user)
    {
        if(user?.Id == null) return;

        try
        {
            _editingUserId = user.Id;
            DialogTitle    = _localizer["Manage Roles"];
            DialogType     = "Roles";

            // Load available roles
            List<string>? rolesResponse = await _apiClient.Users.Roles.GetAsync();
            AvailableRoles.Clear();

            if(rolesResponse != null)
            {
                foreach(string role in rolesResponse)
                    if(!string.IsNullOrWhiteSpace(role))
                        AvailableRoles.Add(role);
            }

            _logger.LogInformation($"Loaded {AvailableRoles.Count} available roles");

            // Load user's current roles
            UserRoles.Clear();

            if(user.Roles != null)
                foreach(string role in user.Roles)
                    UserRoles.Add(role);

            ShowDialogRequested?.Invoke(this, "Roles");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error opening manage roles dialog");
        }
    }

    private async Task SaveUserAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(UserName))
            {
                ErrorMessage = _localizer["Email and username are required."];
                HasError     = true;

                return;
            }

            if(_editingUserId == null)
            {
                // Create new user
                if(string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = _localizer["Password is required for new users."];
                    HasError     = true;

                    return;
                }

                if(Password != ConfirmPassword)
                {
                    ErrorMessage = _localizer["Passwords do not match."];
                    HasError     = true;

                    return;
                }

                var createRequest = new CreateUserRequest
                {
                    Email       = Email,
                    UserName    = UserName,
                    PhoneNumber = PhoneNumber,
                    Password    = Password
                };

                await _apiClient.Users.PostAsync(createRequest);
            }
            else
            {
                // Update existing user
                var updateRequest = new UpdateUserRequest
                {
                    Email       = Email,
                    UserName    = UserName,
                    PhoneNumber = PhoneNumber
                };

                await _apiClient.Users[_editingUserId].PutAsync(updateRequest);
            }

            CloseDialog();
            await LoadUsersAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving user");
            ErrorMessage = _localizer["Failed to save user."];
            HasError     = true;
        }
    }

    private async Task SavePasswordAsync()
    {
        if(_editingUserId == null) return;

        try
        {
            if(string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = _localizer["Password is required."];
                HasError     = true;

                return;
            }

            if(Password != ConfirmPassword)
            {
                ErrorMessage = _localizer["Passwords do not match."];
                HasError     = true;

                return;
            }

            var changePasswordRequest = new ChangePasswordRequest
            {
                NewPassword = Password
            };

            await _apiClient.Users[_editingUserId].Password.PostAsync(changePasswordRequest);

            CloseDialog();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            ErrorMessage = _localizer["Failed to change password."];
            HasError     = true;
        }
    }

    private async Task AddRoleAsync()
    {
        if(_editingUserId == null || string.IsNullOrWhiteSpace(SelectedRole)) return;

        try
        {
            if(UserRoles.Contains(SelectedRole))
            {
                ErrorMessage = _localizer["User already has this role."];
                HasError     = true;

                return;
            }

            var addRoleRequest = new UserRoleRequest
            {
                RoleName = SelectedRole
            };

            await _apiClient.Users[_editingUserId].Roles.PostAsync(addRoleRequest);
            UserRoles.Add(SelectedRole);

            // Reload users to refresh the list
            await LoadUsersAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding role");
            ErrorMessage = _localizer["Failed to add role."];
            HasError     = true;
        }
    }

    private async Task RemoveRoleAsync(string? role)
    {
        if(_editingUserId == null || string.IsNullOrWhiteSpace(role)) return;

        try
        {
            await _apiClient.Users[_editingUserId].Roles[role].DeleteAsync();
            UserRoles.Remove(role);

            // Reload users to refresh the list
            await LoadUsersAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing role");
            ErrorMessage = _localizer["Failed to remove role."];
            HasError     = true;
        }
    }

    private void CloseDialog()
    {
        DialogType      = string.Empty;
        _editingUserId  = null;
        Email           = string.Empty;
        UserName        = string.Empty;
        PhoneNumber     = string.Empty;
        Password        = string.Empty;
        ConfirmPassword = string.Empty;
        UserRoles.Clear();
        AvailableRoles.Clear();
        HasError     = false;
        ErrorMessage = string.Empty;
    }
}