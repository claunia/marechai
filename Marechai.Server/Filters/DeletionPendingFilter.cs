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

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Filters;

/// <summary>
///     Global authorization filter that blocks requests from authenticated users who have an in-flight
///     GDPR deletion request, except where the action is decorated with
///     <see cref="AllowDeletionPendingAttribute" />. Returns a structured <c>403 Forbidden</c> with a
///     custom <c>type=deletion-pending</c> problem-details so the client can navigate to the
///     "Your account is scheduled for deletion" landing page instead of showing a generic error.
/// </summary>
public sealed class DeletionPendingFilter(MarechaiContext context) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
    {
        // No identity yet (anonymous endpoint, login flow, etc.) &mdash; let the request through.
        ClaimsPrincipal user = filterContext.HttpContext.User;

        if(user?.Identity?.IsAuthenticated != true) return;

        // Endpoint explicitly opts in to running while the user is in the grace window. Status, cancel
        // and data-export must remain reachable.
        if(filterContext.ActionDescriptor.EndpointMetadata.OfType<AllowDeletionPendingAttribute>().Any()) return;

        string userId = user.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return;

        // Cache the lookup result on HttpContext.Items so multi-filter requests don't repeat the query.
        const string CacheKey = "DeletionPendingFilter.Result";

        bool isPending;

        if(filterContext.HttpContext.Items.TryGetValue(CacheKey, out object cached) && cached is bool cachedFlag)
        {
            isPending = cachedFlag;
        }
        else
        {
            System.DateTime? requestedAt = await context.Users
                                                        .Where(u => u.Id == userId)
                                                        .Select(u => u.DeletionRequestedAt)
                                                        .FirstOrDefaultAsync();

            isPending                                  = requestedAt.HasValue;
            filterContext.HttpContext.Items[CacheKey]  = isPending;
        }

        if(!isPending) return;

        filterContext.Result = new ObjectResult(new
                                                {
                                                    type   = "deletion-pending",
                                                    title  = "Account scheduled for deletion",
                                                    status = StatusCodes.Status403Forbidden,
                                                    detail = "This account is in the GDPR deletion grace window and cannot perform this action. Cancel the deletion request to restore normal access."
                                                })
                               {
                                   StatusCode  = StatusCodes.Status403Forbidden,
                                   ContentTypes = { "application/problem+json" }
                               };
    }
}
