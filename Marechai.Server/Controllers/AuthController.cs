/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Marechai.Data.Constants;
using Marechai.Data.Models;
using Marechai.Database.Models;
using Marechai.Email.Composers;
using Marechai.Helpers;
using Marechai.Server.Localization;
using Marechai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Marechai.Server.Controllers;

[ApiController]
[Route("auth")]
public class AuthController
    (UserManager<ApplicationUser> userManager,         MarechaiContext              context,
     TokenService                 tokenService,        IConfiguration               configuration,
     TwoFactorEmailComposer       twoFactorEmailComposer,
     PasswordResetEmailComposer   passwordResetEmailComposer) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(AuthResponse),
                          StatusCodes.Status200OK,
                          Description =
                              "Authenticates a user with email and password, returning an access token if successful.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser managedUser = await userManager.FindByEmailAsync(request.Email);

        if(managedUser == null) return Unauthorized("Bad credentials");

        bool isPasswordValid = await userManager.CheckPasswordAsync(managedUser, request.Password);

        if(!isPasswordValid) return Unauthorized("Bad credentials");

        ApplicationUser userInDb = context.Users.FirstOrDefault(u => u.Email == request.Email);

        if(userInDb is null) return Unauthorized();

        // Two-factor required: short-circuit and return a pending token. The client must collect a code via one of
        // the /auth/login/two-factor* endpoints to complete the login.
        if(userInDb.TwoFactorEnabled)
        {
            var availableMethods = new List<string>();
            if(userInDb.TwoFactorViaAuthenticator) availableMethods.Add("authenticator");
            if(userInDb.TwoFactorViaEmail)         availableMethods.Add("email");

            // Defensive: if global TwoFactorEnabled is somehow set without any method flagged (e.g. an admin
            // partial-disabled the user mid-state), fall through to the normal token issuance rather than locking
            // the account out completely.
            if(availableMethods.Count > 0)
            {
                string pendingToken = tokenService.CreatePending2FaToken(userInDb.Id);

                return Ok(new AuthResponse
                {
                    Message           = "",
                    Succeeded         = true,
                    Token             = "",
                    RequiresTwoFactor = true,
                    TwoFactorToken    = pendingToken,
                    AvailableMethods  = availableMethods
                });
            }
        }

        string accessToken = tokenService.CreateToken(userInDb, await userManager.GetRolesAsync(managedUser));
        await context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Message   = "",
            Succeeded = true,
            Token     = accessToken
        });
    }

    [HttpGet]
    [Route("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK, Description = "Returns the current user's profile.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        IList<string> roles = await userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id                    = user.Id,
            UserName              = user.UserName!,
            Email                 = user.Email!,
            EmailConfirmed        = user.EmailConfirmed,
            PhoneNumber           = user.PhoneNumber,
            PhoneNumberConfirmed  = user.PhoneNumberConfirmed,
            LockoutEnabled        = user.LockoutEnabled,
            LockoutEnd            = user.LockoutEnd?.ToString("O"),
            AccessFailedCount     = user.AccessFailedCount,
            PreferredThemeId      = user.PreferredThemeId,
            TwoFactorEnabled      = user.TwoFactorEnabled,
            AuthenticatorEnabled  = user.TwoFactorViaAuthenticator,
            EmailTwoFactorEnabled = user.TwoFactorViaEmail,
            Roles                 = roles.ToList()
        });
    }

    [HttpPut]
    [Route("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK, Description = "Updates the current user's profile.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        user.UserName    = request.UserName;
        user.Email       = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        IdentityResult result = await userManager.UpdateAsync(user);

        if(!result.Succeeded) return BadRequest(result.Errors);

        IList<string> roles = await userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id                    = user.Id,
            UserName              = user.UserName,
            Email                 = user.Email,
            EmailConfirmed        = user.EmailConfirmed,
            PhoneNumber           = user.PhoneNumber,
            PhoneNumberConfirmed  = user.PhoneNumberConfirmed,
            LockoutEnabled        = user.LockoutEnabled,
            LockoutEnd            = user.LockoutEnd?.ToString("O"),
            AccessFailedCount     = user.AccessFailedCount,
            PreferredThemeId      = user.PreferredThemeId,
            TwoFactorEnabled      = user.TwoFactorEnabled,
            AuthenticatorEnabled  = user.TwoFactorViaAuthenticator,
            EmailTwoFactorEnabled = user.TwoFactorViaEmail,
            Roles                 = roles.ToList()
        });
    }

    [HttpPut]
    [Route("me/theme")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Theme preference saved.")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [Consumes("application/json")]
    public async Task<IActionResult> SetMyThemeAsync([FromBody] UpdateUserThemeRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        string requested = string.IsNullOrWhiteSpace(request.ThemeId) ? null : request.ThemeId.Trim();

        if(requested is not null && !ThemeIds.All.Contains(requested))
            return Problem("Unknown theme identifier.",
                           statusCode: StatusCodes.Status400BadRequest,
                           title: "UNKNOWN_THEME");

        user.PreferredThemeId = requested;

        IdentityResult result = await userManager.UpdateAsync(user);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost]
    [Route("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Password changed successfully.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeOwnPasswordRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        IdentityResult result = await userManager.ChangePasswordAsync(user, request.CurrentPassword,
                                                                      request.NewPassword);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost]
    [Route("password/forgot")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent,
                          Description =
                              "Always returned regardless of whether the email matches a registered account, to prevent enumeration. If the address belongs to a confirmed account a reset link is dispatched.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("application/json")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser user = await userManager.FindByEmailAsync(request.Email);

        // Anti-enumeration: silently succeed for unknown or unconfirmed accounts. Lockout-on-success
        // (below) still means an attacker probing valid addresses pays a real cost.
        if(user is null || !user.EmailConfirmed) return NoContent();

        string token = await userManager.GeneratePasswordResetTokenAsync(user);

        string baseUrl = (configuration["Frontend:BaseUrl"] ?? string.Empty).TrimEnd('/');

        string resetUrl =
            $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        await SendLocalizedPasswordResetAsync(user.Email!, resetUrl);

        // Reuse Identity's lockout machinery (same approach as the failed-2FA paths) so an attacker that
        // guesses a valid address can't grind the endpoint indefinitely.
        await userManager.AccessFailedAsync(user);

        return NoContent();
    }

    [HttpPost]
    [Route("password/reset")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Password successfully reset.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("application/json")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser user = await userManager.FindByEmailAsync(request.Email);

        // Generic message: do NOT differentiate between unknown user and bad/expired token.
        if(user is null) return BadRequest("Invalid token or email.");

        IdentityResult result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if(!result.Succeeded)
        {
            await userManager.AccessFailedAsync(user);

            return BadRequest(result.Errors);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return NoContent();
    }

    [HttpGet]
    [Route("me/public-profile")]
    [Authorize]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK,
                          Description = "Returns the current user's public profile.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<ActionResult<PublicProfileDto>> GetPublicProfile()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        return Ok(ProfileController.MapToPublicProfile(user));
    }

    [HttpPut]
    [Route("me/public-profile")]
    [Authorize]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK,
                          Description = "Updates the current user's public profile.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<PublicProfileDto>> UpdatePublicProfile(
        [FromBody] UpdatePublicProfileRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        user.DisplayName = request.DisplayName;
        user.Bio         = request.Bio;
        user.Website     = request.Website;
        user.Location    = request.Location;
        user.UseGravatar = request.UseGravatar;
        user.Twitter     = request.Twitter;
        user.GitHub      = request.GitHub;
        user.Mastodon    = request.Mastodon;
        user.Facebook    = request.Facebook;
        user.LinkedIn    = request.LinkedIn;

        IdentityResult result = await userManager.UpdateAsync(user);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return Ok(ProfileController.MapToPublicProfile(user));
    }

    [HttpPost]
    [Route("me/avatar/upload")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK,
                          Description = "Uploads a custom avatar for the current user.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PublicProfileDto>> UploadAvatarAsync(IFormFile file)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP, TIFF, BMP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        // If avatar already exists, delete old files
        if(user.AvatarGuid.HasValue)
            DeleteAvatarFiles(user.AvatarGuid.Value);

        Guid avatarGuid = Guid.NewGuid();

        Photos.EnsureCreated(_assetRootPath, false, "avatars");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "avatars", "originals");
        string originalPath = Path.Combine(originalsDir, $"{avatarGuid}{extension}");

        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(fs);
        }

        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            var photos = new Photos();
            photos.ConversionWorker(_assetRootPath, avatarGuid, originalPath, sourceFormat, false, "avatars");
        });

        user.AvatarGuid              = avatarGuid;
        user.OriginalAvatarExtension = sourceFormat;

        await userManager.UpdateAsync(user);

        return Ok(ProfileController.MapToPublicProfile(user));
    }

    [HttpDelete]
    [Route("me/avatar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Avatar deleted successfully.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAvatarAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        if(user.AvatarGuid.HasValue)
            DeleteAvatarFiles(user.AvatarGuid.Value);

        user.AvatarGuid              = null;
        user.OriginalAvatarExtension = null;

        await userManager.UpdateAsync(user);

        return NoContent();
    }

    void DeleteAvatarFiles(Guid avatarGuid)
    {
        string photosRoot = Path.Combine(_assetRootPath, "photos", "avatars");

        // Delete original
        string originalsDir = Path.Combine(photosRoot, "originals");

        if(Directory.Exists(originalsDir))
        {
            foreach(string f in Directory.GetFiles(originalsDir, $"{avatarGuid}.*"))
                System.IO.File.Delete(f);
        }

        // Delete all generated variants
        string[] formats = ["jpeg", "webp", "heif", "avif"];
        string[] sizes   = ["hd", "1440p", "4k"];

        foreach(string format in formats)
        {
            foreach(string size in sizes)
            {
                string fullPath  = Path.Combine(photosRoot, format, size, $"{avatarGuid}.*");
                string thumbPath = Path.Combine(photosRoot, "thumbs", format, size, $"{avatarGuid}.*");

                foreach(string f in Directory.GetFiles(Path.GetDirectoryName(fullPath)!,
                                                       Path.GetFileName(fullPath)))
                    System.IO.File.Delete(f);

                string thumbDir = Path.GetDirectoryName(thumbPath)!;

                if(Directory.Exists(thumbDir))
                {
                    foreach(string f in Directory.GetFiles(thumbDir, Path.GetFileName(thumbPath)))
                        System.IO.File.Delete(f);
                }
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------------
    //  Two-factor authentication endpoints
    // ----------------------------------------------------------------------------------------------------------

    /// <summary>
    ///     Sends the 2FA login code in the language picked from the request's <c>Accept-Language</c> header
    ///     (restricted to <see cref="EmailCulture.Supported" />, fallback English). Wraps the composer call in a
    ///     <c>CultureInfo.CurrentUICulture</c> swap so <c>IStringLocalizer&lt;EmailStrings&gt;</c> picks up the
    ///     right resource and restores the previous culture afterwards.
    /// </summary>
    async Task SendLocalizedLoginCodeAsync(string toEmail, string code)
    {
        System.Globalization.CultureInfo previous = System.Globalization.CultureInfo.CurrentUICulture;

        try
        {
            System.Globalization.CultureInfo.CurrentUICulture = EmailCulture.PickFromRequest(Request);
            await twoFactorEmailComposer.SendLoginCodeAsync(toEmail, code);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentUICulture = previous;
        }
    }

    /// <summary>
    ///     Sends the password-reset link in the language picked from the request's <c>Accept-Language</c> header
    ///     (restricted to <see cref="EmailCulture.Supported" />, fallback English). Wraps the composer call in a
    ///     <c>CultureInfo.CurrentUICulture</c> swap so <c>IStringLocalizer&lt;EmailStrings&gt;</c> picks up the
    ///     right resource and restores the previous culture afterwards.
    /// </summary>
    async Task SendLocalizedPasswordResetAsync(string toEmail, string resetUrl)
    {
        System.Globalization.CultureInfo previous = System.Globalization.CultureInfo.CurrentUICulture;

        try
        {
            System.Globalization.CultureInfo.CurrentUICulture = EmailCulture.PickFromRequest(Request);
            await passwordResetEmailComposer.SendResetLinkAsync(toEmail, resetUrl);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentUICulture = previous;
        }
    }

    static string MaskEmail(string email)
    {
        if(string.IsNullOrEmpty(email)) return string.Empty;

        int at = email.IndexOf('@');
        if(at <= 0) return "***";

        string local = email[..at];
        string host  = email[(at + 1)..];

        string maskedLocal = local.Length <= 2
                                 ? new string('*', local.Length)
                                 : local[0] + new string('*', local.Length - 2) + local[^1];

        int dot = host.LastIndexOf('.');

        string maskedHost = dot > 0
                                ? host[0] + new string('*', Math.Max(1, dot - 1)) + host[dot..]
                                : "***";

        return $"{maskedLocal}@{maskedHost}";
    }

    /// <summary>
    ///     Builds a base32-encoded string formatted in 4-character groups for easy manual entry into an
    ///     authenticator app (mirrors the formatting Microsoft's Identity sample uses).
    /// </summary>
    static string FormatSharedKey(string unformattedKey)
    {
        var sb = new StringBuilder();
        int i  = 0;

        while(i + 4 < unformattedKey.Length)
        {
            sb.Append(unformattedKey, i, 4).Append(' ');
            i += 4;
        }

        if(i < unformattedKey.Length) sb.Append(unformattedKey[i..]);

        return sb.ToString().ToLowerInvariant();
    }

    /// <summary>
    ///     Returns the canonical otpauth:// URI Microsoft Authenticator and other RFC 6238 apps consume.
    /// </summary>
    string BuildAuthenticatorUri(string email, string unformattedKey)
    {
        const string issuer  = "Marechai";
        string       account = HttpUtility.UrlEncode(email);

        return
            $"otpauth://totp/{HttpUtility.UrlEncode(issuer)}:{account}?secret={unformattedKey}&issuer={HttpUtility.UrlEncode(issuer)}&digits=6&period=30";
    }

    /// <summary>
    ///     Verifies a code in the context of an authenticated user using one of the active 2FA providers.
    ///     Increments <c>AccessFailedCount</c> on failure (defense against brute-forcing the 6-digit code).
    /// </summary>
    async Task<bool> VerifyOwnTwoFactorCodeAsync(ApplicationUser user, string provider, string code)
    {
        bool ok = provider switch
                  {
                      "authenticator" => await userManager.VerifyTwoFactorTokenAsync(
                                             user, TokenOptions.DefaultAuthenticatorProvider, code),
                      "email" => await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider,
                                                                             code),
                      "recovery" => (await userManager.RedeemTwoFactorRecoveryCodeAsync(user, code)).Succeeded,
                      _          => false
                  };

        if(!ok) await userManager.AccessFailedAsync(user);

        return ok;
    }

    [HttpPost]
    [Route("login/two-factor")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<AuthResponse>> LoginTwoFactor([FromBody] TwoFactorVerifyRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = tokenService.ValidatePending2FaToken(request.TwoFactorToken);
        if(userId is null) return Unauthorized("Invalid or expired two-factor token.");

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        string identityProvider = request.Provider switch
                                  {
                                      "authenticator" => TokenOptions.DefaultAuthenticatorProvider,
                                      "email"         => TokenOptions.DefaultEmailProvider,
                                      _               => null
                                  };

        if(identityProvider is null) return BadRequest("Unknown two-factor provider.");

        bool ok = await userManager.VerifyTwoFactorTokenAsync(user, identityProvider, request.Code);

        if(!ok)
        {
            await userManager.AccessFailedAsync(user);

            return Unauthorized("Invalid verification code.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        string accessToken = tokenService.CreateToken(user, await userManager.GetRolesAsync(user));

        return Ok(new AuthResponse
        {
            Message   = "",
            Succeeded = true,
            Token     = accessToken
        });
    }

    [HttpPost]
    [Route("login/recovery")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<AuthResponse>> LoginRecovery([FromBody] TwoFactorRecoveryRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = tokenService.ValidatePending2FaToken(request.TwoFactorToken);
        if(userId is null) return Unauthorized("Invalid or expired two-factor token.");

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        IdentityResult result = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.RecoveryCode);

        if(!result.Succeeded)
        {
            await userManager.AccessFailedAsync(user);

            return Unauthorized("Invalid recovery code.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        string accessToken = tokenService.CreateToken(user, await userManager.GetRolesAsync(user));

        return Ok(new AuthResponse
        {
            Message   = "",
            Succeeded = true,
            Token     = accessToken
        });
    }

    [HttpPost]
    [Route("login/two-factor/email/send")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TwoFactorEmailSendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<TwoFactorEmailSendResponse>> SendLoginEmailCode(
        [FromBody] TwoFactorEmailSendRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = tokenService.ValidatePending2FaToken(request.TwoFactorToken);
        if(userId is null) return Unauthorized("Invalid or expired two-factor token.");

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        if(!user.TwoFactorViaEmail) return BadRequest("Email two-factor is not enabled for this account.");

        string code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        await SendLocalizedLoginCodeAsync(user.Email!, code);

        return Ok(new TwoFactorEmailSendResponse
        {
            EmailMasked = MaskEmail(user.Email!)
        });
    }

    [HttpGet]
    [Route("me/two-factor/status")]
    [Authorize]
    [ProducesResponseType(typeof(TwoFactorStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<ActionResult<TwoFactorStatusDto>> GetTwoFactorStatus()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        int remaining = await userManager.CountRecoveryCodesAsync(user);

        return Ok(new TwoFactorStatusDto
        {
            Enabled                = user.TwoFactorEnabled,
            AuthenticatorEnabled   = user.TwoFactorViaAuthenticator,
            EmailEnabled           = user.TwoFactorViaEmail,
            RecoveryCodesRemaining = remaining
        });
    }

    [HttpPost]
    [Route("me/two-factor/authenticator/setup")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticatorSetupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<ActionResult<AuthenticatorSetupResponse>> SetupAuthenticator()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        await userManager.ResetAuthenticatorKeyAsync(user);
        string unformatted = await userManager.GetAuthenticatorKeyAsync(user);

        return Ok(new AuthenticatorSetupResponse
        {
            SharedKey        = FormatSharedKey(unformatted),
            AuthenticatorUri = BuildAuthenticatorUri(user.Email!, unformatted)
        });
    }

    [HttpPost]
    [Route("me/two-factor/authenticator/enable")]
    [Authorize]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<RecoveryCodesResponse>> EnableAuthenticator(
        [FromBody] AuthenticatorEnableRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        bool ok = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider,
                                                              request.Code);

        if(!ok) return BadRequest("Invalid verification code.");

        bool wasFirstMethod = !user.TwoFactorEnabled;

        user.TwoFactorViaAuthenticator = true;

        if(wasFirstMethod) await userManager.SetTwoFactorEnabledAsync(user, true);

        await userManager.UpdateAsync(user);

        var response = new RecoveryCodesResponse();

        if(wasFirstMethod)
        {
            IEnumerable<string> codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            response.RecoveryCodes = codes?.ToList() ?? [];
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("me/two-factor/email/start")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StartEmailEnable()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        if(!user.EmailConfirmed) return BadRequest("Email is not confirmed.");

        string code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        await SendLocalizedLoginCodeAsync(user.Email!, code);

        return NoContent();
    }

    [HttpPost]
    [Route("me/two-factor/email/enable")]
    [Authorize]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<RecoveryCodesResponse>> EnableEmail([FromBody] EmailTwoFactorEnableRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        if(!user.EmailConfirmed) return BadRequest("Email is not confirmed.");

        if(!await userManager.CheckPasswordAsync(user, request.Password))
            return BadRequest("Invalid password.");

        bool ok = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, request.Code);
        if(!ok) return BadRequest("Invalid verification code.");

        bool wasFirstMethod = !user.TwoFactorEnabled;

        user.TwoFactorViaEmail = true;
        if(wasFirstMethod) await userManager.SetTwoFactorEnabledAsync(user, true);
        await userManager.UpdateAsync(user);

        var response = new RecoveryCodesResponse();

        if(wasFirstMethod)
        {
            IEnumerable<string> codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            response.RecoveryCodes = codes?.ToList() ?? [];
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("me/two-factor/authenticator/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    public async Task<IActionResult> DisableAuthenticator([FromBody] DisableTwoFactorRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        if(!user.TwoFactorViaAuthenticator) return BadRequest("Authenticator is not enabled.");

        if(!await userManager.CheckPasswordAsync(user, request.Password))
            return BadRequest("Invalid password.");

        if(!await VerifyOwnTwoFactorCodeAsync(user, request.Provider, request.Code))
            return BadRequest("Invalid verification code.");

        await userManager.ResetAuthenticatorKeyAsync(user);
        user.TwoFactorViaAuthenticator = false;

        if(!user.TwoFactorViaEmail)
        {
            await userManager.SetTwoFactorEnabledAsync(user, false);
            await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);
        }

        await userManager.UpdateAsync(user);
        await userManager.ResetAccessFailedCountAsync(user);

        return NoContent();
    }

    [HttpPost]
    [Route("me/two-factor/email/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    public async Task<IActionResult> DisableEmail([FromBody] DisableTwoFactorRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        if(!user.TwoFactorViaEmail) return BadRequest("Email two-factor is not enabled.");

        if(!await userManager.CheckPasswordAsync(user, request.Password))
            return BadRequest("Invalid password.");

        if(!await VerifyOwnTwoFactorCodeAsync(user, request.Provider, request.Code))
            return BadRequest("Invalid verification code.");

        user.TwoFactorViaEmail = false;

        if(!user.TwoFactorViaAuthenticator)
        {
            await userManager.SetTwoFactorEnabledAsync(user, false);
            await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);
        }

        await userManager.UpdateAsync(user);
        await userManager.ResetAccessFailedCountAsync(user);

        return NoContent();
    }

    [HttpPost]
    [Route("me/two-factor/recovery-codes/regenerate")]
    [Authorize]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<RecoveryCodesResponse>> RegenerateRecoveryCodes(
        [FromBody] DisableTwoFactorRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);
        if(user is null) return Unauthorized();

        if(!user.TwoFactorEnabled) return BadRequest("Two-factor authentication is not enabled.");

        if(!await userManager.CheckPasswordAsync(user, request.Password))
            return BadRequest("Invalid password.");

        if(!await VerifyOwnTwoFactorCodeAsync(user, request.Provider, request.Code))
            return BadRequest("Invalid verification code.");

        IEnumerable<string> codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        await userManager.ResetAccessFailedCountAsync(user);

        return Ok(new RecoveryCodesResponse
        {
            RecoveryCodes = codes?.ToList() ?? []
        });
    }
}