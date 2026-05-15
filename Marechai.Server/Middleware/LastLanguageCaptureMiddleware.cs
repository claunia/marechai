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
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Server.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Marechai.Server.Middleware;

/// <summary>
///     Captures the BCP-47 language tag advertised in <c>Accept-Language</c> on every authenticated request and
///     persists it to <c>ApplicationUser.LastLanguageVisited</c> so background workers (notably the new-message
///     email composer) can render content in the user's preferred language. Maintains a process-wide cache of
///     the last value seen per user so the database is only touched when the language actually changes,
///     keeping the per-request overhead at one dictionary lookup.
/// </summary>
public sealed class LastLanguageCaptureMiddleware(
    RequestDelegate                              next,
    ILogger<LastLanguageCaptureMiddleware>       logger)
{
    static readonly ConcurrentDictionary<string, string> _lastSeenByUserId = new(StringComparer.Ordinal);

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        await next(context);

        // Run AFTER the pipeline so we don't block the response on a database write. Authentication state is
        // populated before the controllers execute, so by this point User.Identity is final.

        if(context.User?.Identity?.IsAuthenticated != true) return;

        string userId = context.User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return;

        string requested = ParseAcceptLanguage(context.Request.Headers.AcceptLanguage.ToString());

        if(string.IsNullOrEmpty(requested)) return;

        // Cheap fast-path: if we already saw this exact tag for this user, do nothing.
        if(_lastSeenByUserId.TryGetValue(userId, out string cached) &&
           string.Equals(cached, requested, StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);

            if(user is null) return;

            if(string.Equals(user.LastLanguageVisited, requested, StringComparison.OrdinalIgnoreCase))
            {
                _lastSeenByUserId[userId] = requested;
                return;
            }

            user.LastLanguageVisited = requested;
            await userManager.UpdateAsync(user);

            _lastSeenByUserId[userId] = requested;
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex,
                              "Failed to persist LastLanguageVisited={Language} for user {UserId}",
                              requested, userId);
        }
    }

    /// <summary>
    ///     Picks the highest-quality language tag from an <c>Accept-Language</c> header that maps to one of the
    ///     <see cref="EmailCulture.Supported" /> codes, normalised to the form Marechai stores
    ///     (<c>pt-BR</c> kept as-is, every other regional suffix collapsed to its 2-letter language). Returns
    ///     <c>null</c> when nothing matches &mdash; the caller treats null as "do not update".
    /// </summary>
    static string ParseAcceptLanguage(string header)
    {
        if(string.IsNullOrWhiteSpace(header)) return null;

        System.Collections.Generic.IList<StringWithQualityHeaderValue> parsed;

        try
        {
            parsed = StringWithQualityHeaderValue.ParseList(new[] { header });
        }
        catch
        {
            return null;
        }

        foreach(StringWithQualityHeaderValue entry in parsed.OrderByDescending(e => e.Quality ?? 1.0))
        {
            string tag = entry.Value.Value;

            if(string.IsNullOrWhiteSpace(tag) || tag == "*") continue;

            CultureInfo mapped = EmailCulture.MapToSupported(tag);

            // MapToSupported falls back to "en"; reject the synthetic fallback unless the inbound tag really
            // asked for English (otherwise we'd overwrite genuine "fr"/"es"/etc. with "en" on every header).
            if(mapped.Name == "en" && !tag.StartsWith("en", StringComparison.OrdinalIgnoreCase)) continue;

            return mapped.Name;
        }

        return null;
    }
}
