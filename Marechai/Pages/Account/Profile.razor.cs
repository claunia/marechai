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
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;
using MudBlazor;

namespace Marechai.Pages.Account;

public partial class Profile
{
    // ── Account Settings state ──
    string  _editEmail;
    string  _editPhoneNumber;
    string  _editUserName;
    string  _errorMessage;
    bool     _isEditing;
    bool     _isLoading = true;
    bool     _isSaving;
    UserDto _profile;
    string  _successMessage;

    // ── Public Profile state ──
    PublicProfileDto _publicProfile;
    string           _editDisplayName;
    string           _editBio;
    string           _editWebsite;
    string           _editLocation;
    bool              _editUseGravatar = true;
    string           _editTwitter;
    string           _editGitHub;
    string           _editMastodon;
    string           _editFacebook;
    string           _editLinkedIn;
    bool              _isSavingPublic;
    string           _publicSuccessMessage;
    string           _publicErrorMessage;

    // ── Avatar state ──
    bool      _isUploadingAvatar;
    string   _avatarMessage;
    Severity  _avatarMessageSeverity = Severity.Info;

    // ── Collection state ──
    bool                               _isLoadingCollection;
    List<CollectedBookDto>            _myBooks;
    List<CollectedDocumentDto>        _myDocuments;
    List<CollectedMagazineIssueDto>   _myMagazineIssues;
    List<CollectedMachineDto>         _myMachines;
    List<CollectedSoftwareReleaseDto> _myReleases;

    // ── Suggestions state ──
    List<SuggestionDto> _mySuggestions = new();

    // ── Appearance / theme state ──
    string _savedThemeId;
    string _pendingThemeId;
    bool   _isSavingTheme;
    string _themeSuccessMessage;
    string _themeErrorMessage;

    // ── Security / 2FA state ──
    TwoFactorStatusDto         _twoFactorStatus;
    AuthenticatorSetupResponse _authSetup;
    string                     _authQrSvg = string.Empty;
    string                     _authVerifyCode;
    bool                       _emailEnableMode;
    string                     _emailEnablePassword;
    string                     _emailEnableCode;
    IList<string>              _displayedRecoveryCodes;
    string                     _securityMessage;
    Severity                   _securitySeverity = Severity.Info;
    bool                       _isSecurityBusy;

    protected override async Task OnInitializedAsync()
    {
        _profile         = await AuthService.GetProfileAsync();
        _publicProfile   = await AuthService.GetPublicProfileAsync();
        _twoFactorStatus = await AuthService.GetTwoFactorStatusAsync();

        if(_publicProfile is not null)
            PopulatePublicProfileFields();

        _savedThemeId = _profile?.PreferredThemeId;

        _isLoading = false;

        // Load collection in background
        _ = LoadCollectionAsync();
    }

    void PopulatePublicProfileFields()
    {
        _editDisplayName = _publicProfile?.DisplayName;
        _editBio         = _publicProfile?.Bio;
        _editWebsite     = _publicProfile?.Website;
        _editLocation    = _publicProfile?.Location;
        _editUseGravatar = _publicProfile?.UseGravatar ?? true;
        _editTwitter     = _publicProfile?.Twitter;
        _editGitHub      = _publicProfile?.GitHub;
        _editMastodon    = _publicProfile?.Mastodon;
        _editFacebook    = _publicProfile?.Facebook;
        _editLinkedIn    = _publicProfile?.LinkedIn;
    }

    // ── Account Settings methods ──

    void StartEdit()
    {
        _editUserName    = _profile?.UserName;
        _editEmail       = _profile?.Email;
        _editPhoneNumber = _profile?.PhoneNumber;
        _isEditing       = true;
        _errorMessage    = null;
        _successMessage  = null;
    }

    void CancelEdit()
    {
        _isEditing = false;
    }

    async Task SaveProfileAsync()
    {
        if(string.IsNullOrWhiteSpace(_editUserName))
        {
            _errorMessage = "Username is required.";

            return;
        }

        if(string.IsNullOrWhiteSpace(_editEmail))
        {
            _errorMessage = "Email is required.";

            return;
        }

        _isSaving     = true;
        _errorMessage = null;

        (bool succeeded, string errorMessage) =
            await AuthService.UpdateProfileAsync(_editUserName, _editEmail, _editPhoneNumber);

        _isSaving = false;

        if(succeeded)
        {
            _profile        = await AuthService.GetProfileAsync();
            _isEditing      = false;
            _successMessage = "Profile updated successfully.";
        }
        else
        {
            _errorMessage = errorMessage ?? "Failed to update profile.";
        }
    }

    // ── Public Profile methods ──

    async Task SavePublicProfileAsync()
    {
        _isSavingPublic      = true;
        _publicErrorMessage  = null;

        var request = new UpdatePublicProfileRequest
        {
            DisplayName = string.IsNullOrWhiteSpace(_editDisplayName) ? null : _editDisplayName.Trim(),
            Bio         = string.IsNullOrWhiteSpace(_editBio) ? null : _editBio.Trim(),
            Website     = string.IsNullOrWhiteSpace(_editWebsite) ? null : _editWebsite.Trim(),
            Location    = string.IsNullOrWhiteSpace(_editLocation) ? null : _editLocation.Trim(),
            UseGravatar = _editUseGravatar,
            Twitter     = string.IsNullOrWhiteSpace(_editTwitter) ? null : _editTwitter.Trim(),
            GitHub      = string.IsNullOrWhiteSpace(_editGitHub) ? null : _editGitHub.Trim(),
            Mastodon    = string.IsNullOrWhiteSpace(_editMastodon) ? null : _editMastodon.Trim(),
            Facebook    = string.IsNullOrWhiteSpace(_editFacebook) ? null : _editFacebook.Trim(),
            LinkedIn    = string.IsNullOrWhiteSpace(_editLinkedIn) ? null : _editLinkedIn.Trim()
        };

        (bool succeeded, string errorMessage) = await AuthService.UpdatePublicProfileAsync(request);

        _isSavingPublic = false;

        if(succeeded)
        {
            _publicProfile        = await AuthService.GetPublicProfileAsync();
            _publicSuccessMessage = "Public profile updated successfully.";

            if(_publicProfile is not null)
                PopulatePublicProfileFields();
        }
        else
        {
            _publicErrorMessage = errorMessage ?? "Failed to update public profile.";
        }
    }

    // ── Avatar methods ──

    async Task OnAvatarFileSelected(IBrowserFile file)
    {
        if(file is null) return;

        _isUploadingAvatar = true;
        _avatarMessage     = null;
        StateHasChanged();

        try
        {
            await using var stream = file.OpenReadStream(50 * 1024 * 1024);
            using var       ms     = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            string contentType = Path.GetExtension(file.Name)?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            var multipartBody = new MultipartBody();
            multipartBody.AddOrReplacePart("file", contentType, ms, file.Name);

            PublicProfileDto result = await AuthService.UploadAvatarAsync(multipartBody);

            if(result is not null)
            {
                _publicProfile             = result;
                _avatarMessage             = "Avatar uploaded successfully.";
                _avatarMessageSeverity     = Severity.Success;
                PopulatePublicProfileFields();
            }
            else
            {
                _avatarMessage         = "Failed to upload avatar.";
                _avatarMessageSeverity = Severity.Error;
            }
        }
        catch(Exception ex)
        {
            _avatarMessage         = ex.Message;
            _avatarMessageSeverity = Severity.Error;
        }
        finally
        {
            _isUploadingAvatar = false;
            StateHasChanged();
        }
    }

    async Task DeleteAvatar()
    {
        _isUploadingAvatar = true;
        _avatarMessage     = null;
        StateHasChanged();

        (bool succeeded, string error) = await AuthService.DeleteAvatarAsync();

        if(succeeded)
        {
            _publicProfile         = await AuthService.GetPublicProfileAsync();
            _avatarMessage         = "Avatar deleted.";
            _avatarMessageSeverity = Severity.Success;

            if(_publicProfile is not null)
                PopulatePublicProfileFields();
        }
        else
        {
            _avatarMessage         = error ?? "Failed to delete avatar.";
            _avatarMessageSeverity = Severity.Error;
        }

        _isUploadingAvatar = false;
        StateHasChanged();
    }

    // ── Collection methods ──

    async Task LoadCollectionAsync()
    {
        if(_profile?.UserName is null) return;

        _isLoadingCollection = true;
        StateHasChanged();

        _myBooks     = await CollectionSvc.GetCollectedBooksAsync(_profile.UserName);
        _myDocuments = await CollectionSvc.GetCollectedDocumentsAsync(_profile.UserName);
        _myMachines  = await CollectionSvc.GetCollectedMachinesAsync(_profile.UserName);
        _myReleases  = await CollectionSvc.GetCollectedSoftwareReleasesAsync(_profile.UserName);
        _myMagazineIssues = await CollectionSvc.GetCollectedMagazineIssuesAsync(_profile.UserName);

        // Also load my suggestions on the same render pass.
        _mySuggestions = await SuggestionsSvc.GetMyAsync();

        _isLoadingCollection = false;
        StateHasChanged();
    }

    async Task WithdrawSuggestionAsync(SuggestionDto suggestion)
    {
        if(suggestion?.Id is null) return;

        (bool success, _) = await SuggestionsSvc.WithdrawAsync(suggestion.Id.Value);

        if(success)
        {
            // Re-fetch the list so status reflects the soft-withdraw (Pending → Withdrawn).
            _mySuggestions = await SuggestionsSvc.GetMyAsync();
            StateHasChanged();
        }
    }

    MudBlazor.Color SuggestionStatusColor(Marechai.Data.SuggestionStatus? s) => s switch
    {
        Marechai.Data.SuggestionStatus.Pending           => MudBlazor.Color.Warning,
        Marechai.Data.SuggestionStatus.Accepted          => MudBlazor.Color.Success,
        Marechai.Data.SuggestionStatus.PartiallyAccepted => MudBlazor.Color.Info,
        Marechai.Data.SuggestionStatus.Rejected          => MudBlazor.Color.Error,
        Marechai.Data.SuggestionStatus.Stale             => MudBlazor.Color.Default,
        Marechai.Data.SuggestionStatus.Withdrawn         => MudBlazor.Color.Default,
        _                                                 => MudBlazor.Color.Default
    };

    string SuggestionStatusLabel(Marechai.Data.SuggestionStatus? s) => s switch
    {
        Marechai.Data.SuggestionStatus.Pending           => L["Pending"],
        Marechai.Data.SuggestionStatus.Accepted          => L["Accepted"],
        Marechai.Data.SuggestionStatus.PartiallyAccepted => L["Partially accepted"],
        Marechai.Data.SuggestionStatus.Rejected          => L["Rejected"],
        Marechai.Data.SuggestionStatus.Stale             => L["Stale"],
        Marechai.Data.SuggestionStatus.Withdrawn         => L["Withdrawn"],
        _                                                 => "—"
    };

    /// <summary>
    ///     Compact secondary tag for per-subkey suggestions (e.g. <c>"(Spanish description)"</c>
    ///     for a <see cref="Marechai.Data.SuggestionEntityType.CompanyDescription" /> entry).
    /// </summary>
    static string SuggestionSubkeyLabel(Marechai.ApiClient.Models.SuggestionDto s)
    {
        if(string.IsNullOrEmpty(s.Subkey)) return string.Empty;

        if(s.EntityType == (int?)Marechai.Data.SuggestionEntityType.CompanyDescription)
            return $"({SuggestionLanguageDisplayName(s.Subkey)} description)";

        return $"({s.Subkey})";
    }

    static string SuggestionLanguageDisplayName(string iso639_3) => iso639_3 switch
    {
        "eng" => "English",
        "spa" => "Spanish",
        "deu" => "German",
        "fra" => "French",
        "ita" => "Italian",
        "lat" => "Latin",
        "por" => "Portuguese",
        _     => iso639_3
    };

    async Task RemoveBookAsync(long? bookId)
    {
        if(bookId is null) return;

        (bool success, _) = await CollectionSvc.RemoveBookFromCollectionAsync(bookId.Value);

        if(success)
            _myBooks?.RemoveAll(b => b.BookId == bookId);
    }

    async Task RemoveDocumentAsync(long? documentId)
    {
        if(documentId is null) return;

        (bool success, _) = await CollectionSvc.RemoveDocumentFromCollectionAsync(documentId.Value);

        if(success)
            _myDocuments?.RemoveAll(d => d.DocumentId == documentId);
    }

    async Task RemoveMachineAsync(int? machineId)
    {
        if(machineId is null) return;

        (bool success, _) = await CollectionSvc.RemoveMachineFromCollectionAsync(machineId.Value);

        if(success)
            _myMachines?.RemoveAll(m => m.MachineId == machineId);
    }

    async Task RemoveReleaseAsync(int? releaseId)
    {
        if(releaseId is null) return;

        (bool success, _) = await CollectionSvc.RemoveSoftwareReleaseFromCollectionAsync(releaseId.Value);

        if(success)
            _myReleases?.RemoveAll(r => r.SoftwareReleaseId == releaseId);
    }

    async Task RemoveMagazineIssueAsync(long? issueId)
    {
        if(issueId is null) return;

        (bool success, _) = await CollectionSvc.RemoveMagazineIssueFromCollectionAsync(issueId.Value);

        if(success)
            _myMagazineIssues?.RemoveAll(i => i.MagazineIssueId == issueId);
    }

    // ── Appearance / theme methods ──

    async Task SelectThemeAsync(ThemeDefinition theme)
    {
        if(theme is null || _isSavingTheme) return;

        if(string.Equals(theme.Id, ThemeState.CurrentTheme.Id, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(theme.Id, _savedThemeId,              StringComparison.OrdinalIgnoreCase))
            return;

        ThemeDefinition previous = ThemeState.CurrentTheme;
        string          previousSavedId = _savedThemeId;

        _themeErrorMessage   = null;
        _themeSuccessMessage = null;
        _pendingThemeId      = theme.Id;
        _isSavingTheme       = true;

        // Live preview before the server confirms.
        ThemeState.Set(theme);
        StateHasChanged();

        (bool ok, string error) = await AuthService.SetThemeAsync(theme.Id);

        _isSavingTheme  = false;
        _pendingThemeId = null;

        if(ok)
        {
            _savedThemeId        = theme.Id;
            _themeSuccessMessage = $"Theme set to “{theme.DisplayName}”.";
        }
        else
        {
            // Roll back the live preview if the server refused.
            ThemeState.Set(previous);
            _savedThemeId      = previousSavedId;
            _themeErrorMessage = error ?? "Failed to save theme.";
        }

        StateHasChanged();
    }

    async Task ResetThemeAsync()
    {
        if(_isSavingTheme) return;

        ThemeDefinition previous        = ThemeState.CurrentTheme;
        string          previousSavedId = _savedThemeId;

        _themeErrorMessage   = null;
        _themeSuccessMessage = null;
        _isSavingTheme       = true;
        StateHasChanged();

        (bool ok, string error) = await AuthService.SetThemeAsync(null);

        _isSavingTheme = false;

        if(ok)
        {
            ThemeState.Set(ThemeCatalog.Default);
            _savedThemeId        = null;
            _themeSuccessMessage = "Theme reset to default.";
        }
        else
        {
            ThemeState.Set(previous);
            _savedThemeId      = previousSavedId;
            _themeErrorMessage = error ?? "Failed to reset theme.";
        }

        StateHasChanged();
    }

    // ── Security / 2FA methods ──

    async Task RefreshTwoFactorStatusAsync()
    {
        _twoFactorStatus = await AuthService.GetTwoFactorStatusAsync();
        StateHasChanged();
    }

    async Task StartAuthenticatorSetupAsync()
    {
        _isSecurityBusy  = true;
        _securityMessage = null;

        AuthenticatorSetupResponse setup = await AuthService.SetupAuthenticatorAsync();

        _isSecurityBusy = false;

        if(setup is null || string.IsNullOrEmpty(setup.AuthenticatorUri))
        {
            _securityMessage  = "Failed to generate authenticator setup.";
            _securitySeverity = Severity.Error;

            return;
        }

        _authSetup = setup;
        _authQrSvg = BuildQrSvg(setup.AuthenticatorUri);
    }

    static string BuildQrSvg(string text)
    {
        using var generator = new QRCoder.QRCodeGenerator();
        QRCoder.QRCodeData data = generator.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
        var               svg  = new QRCoder.SvgQRCode(data);

        // 4 px per module gives ~120-160 px QR codes which fit nicely in the profile card.
        return svg.GetGraphic(4);
    }

    async Task EnableAuthenticatorAsync()
    {
        if(string.IsNullOrWhiteSpace(_authVerifyCode))
        {
            _securityMessage  = "Verification code is required.";
            _securitySeverity = Severity.Error;

            return;
        }

        _isSecurityBusy = true;

        (bool ok, IList<string> codes, string err) = await AuthService.EnableAuthenticatorAsync(_authVerifyCode.Trim());

        _isSecurityBusy = false;

        if(!ok)
        {
            _securityMessage  = err ?? "Invalid verification code.";
            _securitySeverity = Severity.Error;

            return;
        }

        _authSetup       = null;
        _authQrSvg       = string.Empty;
        _authVerifyCode  = null;
        _securityMessage = "Authenticator enabled.";
        _securitySeverity = Severity.Success;

        if(codes is { Count: > 0 }) _displayedRecoveryCodes = codes;

        await RefreshTwoFactorStatusAsync();
    }

    async Task StartEmailEnableAsync()
    {
        _isSecurityBusy = true;
        (bool ok, string err) = await AuthService.StartEmailEnableAsync();
        _isSecurityBusy = false;

        if(ok)
        {
            _emailEnableMode  = true;
            _securityMessage  = "Code sent to your email.";
            _securitySeverity = Severity.Info;
        }
        else
        {
            _securityMessage  = err ?? "Could not send code.";
            _securitySeverity = Severity.Error;
        }
    }

    async Task EnableEmailAsync()
    {
        if(string.IsNullOrWhiteSpace(_emailEnablePassword) || string.IsNullOrWhiteSpace(_emailEnableCode))
        {
            _securityMessage  = "Password and code are required.";
            _securitySeverity = Severity.Error;

            return;
        }

        _isSecurityBusy = true;

        (bool ok, IList<string> codes, string err) = await AuthService.EnableEmailAsync(_emailEnablePassword,
                                                                                          _emailEnableCode.Trim());

        _isSecurityBusy = false;

        if(!ok)
        {
            _securityMessage  = err ?? "Invalid verification code.";
            _securitySeverity = Severity.Error;

            return;
        }

        _emailEnableMode     = false;
        _emailEnablePassword = null;
        _emailEnableCode     = null;
        _securityMessage     = "Email two-factor enabled.";
        _securitySeverity    = Severity.Success;

        if(codes is { Count: > 0 }) _displayedRecoveryCodes = codes;

        await RefreshTwoFactorStatusAsync();
    }

    async Task OpenDisableAuthenticatorDialog()
    {
        var result = await ShowDisableDialog("Disable authenticator app");

        if(result is null) return;

        _isSecurityBusy = true;
        (bool ok, string err) = await AuthService.DisableAuthenticatorAsync(result.Password, result.Code,
                                                                            result.Provider);
        _isSecurityBusy = false;

        _securityMessage  = ok ? "Authenticator disabled." : err ?? "Failed to disable authenticator.";
        _securitySeverity = ok ? Severity.Success : Severity.Error;

        if(ok) await RefreshTwoFactorStatusAsync();
    }

    async Task OpenDisableEmailDialog()
    {
        var result = await ShowDisableDialog("Disable email two-factor");

        if(result is null) return;

        _isSecurityBusy = true;
        (bool ok, string err) = await AuthService.DisableEmailAsync(result.Password, result.Code, result.Provider);
        _isSecurityBusy = false;

        _securityMessage  = ok ? "Email two-factor disabled." : err ?? "Failed to disable email two-factor.";
        _securitySeverity = ok ? Severity.Success : Severity.Error;

        if(ok) await RefreshTwoFactorStatusAsync();
    }

    async Task OpenRegenerateRecoveryDialog()
    {
        var result = await ShowDisableDialog("Regenerate recovery codes");

        if(result is null) return;

        _isSecurityBusy = true;

        (bool ok, IList<string> codes, string err) =
            await AuthService.RegenerateRecoveryCodesAsync(result.Password, result.Code, result.Provider);

        _isSecurityBusy = false;

        if(ok)
        {
            _displayedRecoveryCodes = codes;
            _securityMessage        = "Recovery codes regenerated.";
            _securitySeverity       = Severity.Success;
            await RefreshTwoFactorStatusAsync();
        }
        else
        {
            _securityMessage  = err ?? "Failed to regenerate recovery codes.";
            _securitySeverity = Severity.Error;
        }
    }

    async Task<DisableTwoFactorPrompt> ShowDisableDialog(string title)
    {
        var defaultProvider = _twoFactorStatus?.AuthenticatorEnabled == true ? "authenticator" : "email";

        var parameters = new DialogParameters<DisableTwoFactorDialog>
        {
            { x => x.Title,                  title },
            { x => x.AuthenticatorAvailable, _twoFactorStatus?.AuthenticatorEnabled == true },
            { x => x.EmailAvailable,         _twoFactorStatus?.EmailEnabled         == true },
            { x => x.DefaultProvider,        defaultProvider }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DisableTwoFactorDialog>(title, parameters);
        DialogResult     dr     = await dialog.Result;

        if(dr is { Canceled: false, Data: DisableTwoFactorPrompt data }) return data;

        return null;
    }
}

public sealed class DisableTwoFactorPrompt
{
    public string Password { get; set; }
    public string Code     { get; set; }
    public string Provider { get; set; }
}
