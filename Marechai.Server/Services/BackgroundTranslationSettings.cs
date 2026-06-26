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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Marechai.Server.Services;

/// <summary>
///     Shared helpers for the server background translation workers/providers. Keeps the pause +
///     bounded-wave behavior consistent across the short-text and long-form translation paths.
/// </summary>
public static class BackgroundTranslationSettings
{
    static readonly TimeSpan _pausedPollInterval = TimeSpan.FromSeconds(1);

    public static int NormalizeMaxParallelTranslations(int configuredValue) =>
        configuredValue < 0 ? 0 : configuredValue;

    public static int GetMaxParallelTranslations(IOptionsMonitor<TranslationOptions> options) =>
        NormalizeMaxParallelTranslations(options.CurrentValue?.MaxParallelTranslations ?? 1);

    public static async Task<int> WaitForAvailableParallelismAsync(IOptionsMonitor<TranslationOptions> options,
                                                                   CancellationToken                    ct)
    {
        while(true)
        {
            int maxParallelTranslations = GetMaxParallelTranslations(options);

            if(maxParallelTranslations > 0) return maxParallelTranslations;

            await Task.Delay(_pausedPollInterval, ct);
        }
    }

    public static async Task<int> FlushFullBatchesAsync<T>(
        List<T> pending, int flushBatchSize, Func<List<T>, CancellationToken, Task> persistBatchAsync, CancellationToken ct)
    {
        var inserted = 0;

        while(pending.Count >= flushBatchSize)
        {
            List<T> batch = pending.GetRange(0, flushBatchSize);
            await persistBatchAsync(batch, ct);
            pending.RemoveRange(0, flushBatchSize);
            inserted += batch.Count;
        }

        return inserted;
    }
}
