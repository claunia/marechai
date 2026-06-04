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
using System.Threading;

namespace Marechai.Server.Services.Rankings;

/// <summary>
///     Process-lifetime singleton holding the freshness + concurrency state of the persisted
///     Marechai rankings. Surfaces (a) the last completed full-recompute timestamp so the
///     <c>MarechaiRankingsWorker</c> can decide whether the 24 h tick has elapsed since
///     startup, (b) an <see cref="IsComputing" /> flag so the read controllers can include
///     a "rankings are being computed" hint in their first-ever response, and (c) a single
///     <see cref="SemaphoreSlim" /> gate so the worker and the optional manual-recompute
///     endpoint never run two computations in parallel.
/// </summary>
/// <remarks>
///     The <see cref="LastComputedAt" /> in this singleton is an in-memory mirror of the
///     value persisted in <c>RankingDefinitions.ComputedAt</c>; the calculator updates both
///     atomically (the singleton AFTER the DB transaction commits successfully). On a fresh
///     process boot the singleton starts null and the worker reads the DB to seed it.
/// </remarks>
public sealed class RankingsComputationState
{
    /// <summary>
    ///     One-permit semaphore. Both the worker's periodic tick and the optional UberAdmin
    ///     recompute endpoint acquire this before doing anything; a contended caller can
    ///     either wait (worker) or short-circuit with HTTP 409 (controller).
    /// </summary>
    public SemaphoreSlim Gate { get; } = new(1, 1);

    /// <summary>UTC timestamp of the last successful full recompute, or null if never run.</summary>
    public DateTime? LastComputedAt { get; private set; }

    /// <summary>True while the calculator is mid-run (worker OR manual trigger).</summary>
    public bool IsComputing { get; private set; }

    /// <summary>Called by the calculator right after acquiring the gate.</summary>
    public void MarkComputing() => IsComputing = true;

    /// <summary>Called by the calculator on successful completion (BEFORE releasing the gate).</summary>
    public void MarkCompleted(DateTime utcNow)
    {
        LastComputedAt = utcNow;
        IsComputing    = false;
    }

    /// <summary>Called by the calculator on failure / exception.</summary>
    public void MarkFailed() => IsComputing = false;

    /// <summary>Called by the worker on startup once it has read the DB.</summary>
    public void SeedLastComputedAt(DateTime? value) => LastComputedAt = value;
}
