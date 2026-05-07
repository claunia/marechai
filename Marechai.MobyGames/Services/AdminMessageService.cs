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
using System.Threading.Tasks;
using Marechai.Database.Helpers;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Posts system-authored messages to all current Admin/UberAdmin users from the standalone
///     console importer (which has no <c>UserManager</c> wired up). Resolves admin IDs by joining
///     <c>UserRoles</c> + <c>Roles</c> directly off the EF context — both inherited from
///     <see cref="IdentityDbContext{TUser,TRole,TKey}" /> on <see cref="MarechaiContext" />.
/// </summary>
public class AdminMessageService
{
    static readonly string[] AdminRoleNames = ["Admin", "UberAdmin"];

    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public AdminMessageService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    /// <summary>
    ///     Sends a single system message to every Admin/UberAdmin user. Returns <c>true</c> on success
    ///     (including the case where there are no admins — nothing to do is not a failure), <c>false</c>
    ///     on any database/persistence error. Reporting failures must never abort the importer, so all
    ///     exceptions are caught and logged.
    /// </summary>
    public async Task<bool> SendToAdminsAsync(string subject, string body)
    {
        try
        {
            await using MarechaiContext context = await _contextFactory.CreateDbContextAsync();

            string[] adminIds = await (from ur in context.UserRoles
                                       join r in context.Roles on ur.RoleId equals r.Id
                                       where AdminRoleNames.Contains(r.Name)
                                       select ur.UserId).Distinct().ToArrayAsync();

            if(adminIds.Length == 0)
            {
                Console.WriteLine("    [AdminMessageService] No Admin/UberAdmin users found; report skipped.");

                return true;
            }

            await MessageDispatcher.PostSystemMessageAsync(context, adminIds, subject, body);

            Console.WriteLine($"    [AdminMessageService] Sent admin report to {adminIds.Length} user(s).");

            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"    [AdminMessageService] FAILED to send admin report: {ex.Message}");

            return false;
        }
    }

    /// <summary>
    ///     Resolves the <c>Software.Name</c>s of the linked games (using a fresh context so the
    ///     caller's mutation context state can't break us), formats the partial-compilation report
    ///     and posts it to all Admin/UberAdmin users. Always best-effort — never throws.
    /// </summary>
    public async Task<bool> SendCompilationReportAsync(string compilationName, string mobyGameId,
                                                       IReadOnlyCollection<ulong> resolvedSoftwareIds,
                                                       IReadOnlyCollection<string> unresolvedSlugs,
                                                       IReadOnlyCollection<UnresolvableCompilationLink> unresolvableLinks,
                                                       bool compilationCreated)
    {
        try
        {
            // Use a fresh context for the name lookup so we don't depend on any state in the
            // caller's heavy-mutation context (which may have failed or be partially saved).
            var resolvedNames = new List<(ulong Id, string Name)>();

            if(resolvedSoftwareIds is { Count: > 0 })
            {
                var idList = resolvedSoftwareIds.ToList();

                await using MarechaiContext lookupContext = await _contextFactory.CreateDbContextAsync();

                var rows = await lookupContext.Softwares
                                              .AsNoTracking()
                                              .Where(s => idList.Contains(s.Id))
                                              .Select(s => new { s.Id, s.Name })
                                              .ToListAsync();

                var byId = rows.ToDictionary(r => r.Id, r => r.Name);

                foreach(ulong id in idList)
                    resolvedNames.Add((id, byId.TryGetValue(id, out string n) ? n : null));
            }

            string subject = CompilationReportBuilder.BuildSubject(compilationName);

            string body = CompilationReportBuilder.BuildBody(compilationName, mobyGameId, resolvedNames,
                                                             unresolvedSlugs ?? [],
                                                             unresolvableLinks ?? [],
                                                             compilationCreated);

            return await SendToAdminsAsync(subject, body);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"    [AdminMessageService] FAILED to build/send compilation report: {ex.Message}");

            return false;
        }
    }
}

