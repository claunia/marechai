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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marechai.Server.Services.DescriptionTranslation;

/// <summary><see cref="IDescriptionTranslationSource" /> for <see cref="SoftwareDescription" />.</summary>
public sealed class SoftwareDescriptionSource(IServiceScopeFactory scopeFactory) : IDescriptionTranslationSource
{
    public string Name        => "SoftwareDescription";
    public bool   RendersHtml => true;

    public async Task<DescriptionTranslationItem> FetchNextMissingAsync(string languageCode, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        var row = await ctx.SoftwareDescriptions.AsNoTracking()
                           .Where(d => d.LanguageCode == "eng" &&
                                       !ctx.SoftwareDescriptions.Any(t => t.SoftwareId == d.SoftwareId &&
                                                                          t.LanguageCode == languageCode))
                           .OrderBy(d => d.SoftwareId)
                           .Select(d => new
                           {
                               d.SoftwareId,
                               d.Text
                           })
                           .FirstOrDefaultAsync(ct);

        return row is null
                   ? null
                   : new DescriptionTranslationItem(row.SoftwareId, languageCode, row.Text);
    }

    public async Task SaveAsync(DescriptionTranslationItem item, string translatedMarkdown, string translatedHtml,
                                CancellationToken          ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        var entityId = (ulong)item.EntityId;

        // Idempotency guard — between FetchNext and Save another worker / admin save could have
        // inserted the row. Bail out without overwriting.
        bool exists = await ctx.SoftwareDescriptions.AnyAsync(d => d.SoftwareId == entityId &&
                                                                   d.LanguageCode == item.LanguageCode, ct);

        if(exists) return;

        await ctx.SoftwareDescriptions.AddAsync(new SoftwareDescription
        {
            SoftwareId   = entityId,
            LanguageCode = item.LanguageCode,
            Text         = translatedMarkdown,
            Html         = translatedHtml ?? string.Empty
        }, ct);

        await ctx.SaveChangesAsync(ct);
    }
}
