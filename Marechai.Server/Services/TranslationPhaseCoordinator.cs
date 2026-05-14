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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Singleton handshake between the primary <see cref="TranslationWorker" /> and the companion
///     <c>DescriptionTranslationWorker</c>. The description worker only runs while the translation
///     worker is in its inter-sweep slumber; when the translation worker wakes, it asks the
///     description worker to stop after its current in-progress translation and waits (with a hard
///     cap) for an acknowledgement before resuming.
/// </summary>
/// <remarks>
///     Lifecycle of one cycle:
///     <list type="number">
///         <item><see cref="TranslationWorker" /> finishes a sweep and calls <see cref="EnterSlumber" />,
///         receiving a <see cref="CancellationToken" /> that fires when slumber ends.</item>
///         <item><c>DescriptionTranslationWorker</c> awaits <see cref="WaitForSlumberAsync" />, which
///         returns the same slumber CT.</item>
///         <item>When the translation worker is ready to resume, it calls
///         <see cref="ExitSlumberAsync" /> which (a) cancels the slumber CT, (b) awaits
///         <see cref="NotifyDescriptionStopped" /> from the description worker, capped at
///         <see cref="StopHandshakeTimeout" /> so a hung OpenAI call cannot block the wake.</item>
///         <item>Description worker calls <see cref="NotifyDescriptionStopped" /> in its <c>finally</c>
///         and goes back to awaiting the next slumber.</item>
///     </list>
///     <para>Thread-safety: state mutations are guarded by <c>_gate</c>. <see cref="TaskCompletionSource" />
///     instances are recreated each cycle; callers always read them through helpers that take the lock.</para>
/// </remarks>
public sealed class TranslationPhaseCoordinator(ILogger<TranslationPhaseCoordinator> logger)
{
    /// <summary>Hard cap on the wait for a description-stopped acknowledgement before the translation
    /// worker resumes anyway.</summary>
    public static readonly TimeSpan StopHandshakeTimeout = TimeSpan.FromMinutes(5);

    readonly object _gate = new();

    // Resolved when EnterSlumber() is called — the description worker awaits this to know when it
    // can start a slumber pass.
    TaskCompletionSource _slumberStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // Resolved when the description worker calls NotifyDescriptionStopped() — the translation
    // worker awaits this in ExitSlumberAsync() to know the description worker has yielded.
    TaskCompletionSource _descriptionStopped = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // Cancelled when ExitSlumberAsync() is called — the description worker checks this between
    // every fetch/translate iteration so it can break out gracefully.
    CancellationTokenSource _slumberCts = new();

    bool _descriptionActive;

    /// <summary>
    ///     Called by <see cref="TranslationWorker" /> after each sweep completes. Allocates fresh
    ///     handshake primitives for the new cycle and signals the description worker that slumber
    ///     has begun. Returns a <see cref="CancellationToken" /> that fires when slumber ends.
    /// </summary>
    public CancellationToken EnterSlumber()
    {
        TaskCompletionSource startedToSignal;

        lock(_gate)
        {
            // Fresh CTS for this cycle — the previous one (if any) was cancelled by ExitSlumberAsync.
            _slumberCts.Dispose();
            _slumberCts = new CancellationTokenSource();

            // Reset the stop-acknowledged TCS for this cycle.
            _descriptionStopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            // Capture the existing _slumberStarted TCS to signal AFTER releasing the lock; allocate
            // a new one for the NEXT cycle so a description worker that only starts waiting after
            // EnterSlumber returns still gets a fresh handshake.
            startedToSignal = _slumberStarted;
            _slumberStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        startedToSignal.TrySetResult();
        logger.LogDebug("TranslationPhaseCoordinator: slumber started.");

        return _slumberCts.Token;
    }

    /// <summary>
    ///     Called by <see cref="TranslationWorker" /> when its slumber delay expires (or is
    ///     cancelled by host shutdown). Cancels the slumber CT so the description worker breaks out,
    ///     then awaits <see cref="NotifyDescriptionStopped" /> for at most
    ///     <see cref="StopHandshakeTimeout" />. After the handshake (or its timeout) the translation
    ///     worker is free to resume its next sweep.
    /// </summary>
    public async Task ExitSlumberAsync(CancellationToken hostStoppingToken)
    {
        Task                    ackTask;
        bool                    wasActive;
        CancellationTokenSource ctsToCancel;

        lock(_gate)
        {
            wasActive   = _descriptionActive;
            ackTask     = _descriptionStopped.Task;
            ctsToCancel = _slumberCts;
        }

        try
        {
            ctsToCancel.Cancel();
        }
        catch(ObjectDisposedException) { }

        if(!wasActive)
        {
            logger.LogDebug("TranslationPhaseCoordinator: exit slumber — description worker not active.");

            return;
        }

        logger.LogDebug("TranslationPhaseCoordinator: exit slumber — awaiting description worker stop.");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(hostStoppingToken);
        timeoutCts.CancelAfter(StopHandshakeTimeout);

        try
        {
            await ackTask.WaitAsync(timeoutCts.Token);
            logger.LogDebug("TranslationPhaseCoordinator: description worker acknowledged stop.");
        }
        catch(OperationCanceledException) when(hostStoppingToken.IsCancellationRequested)
        {
            // Host is shutting down — nothing left to do.
        }
        catch(TimeoutException)
        {
            logger.LogWarning(
                "TranslationPhaseCoordinator: timed out waiting for description worker stop after {Timeout}; resuming translation worker anyway.",
                StopHandshakeTimeout);
        }
        catch(OperationCanceledException)
        {
            // CancelAfter fires as OperationCanceledException, not TimeoutException — same handling.
            logger.LogWarning(
                "TranslationPhaseCoordinator: timed out waiting for description worker stop after {Timeout}; resuming translation worker anyway.",
                StopHandshakeTimeout);
        }
    }

    /// <summary>
    ///     Called by <c>DescriptionTranslationWorker</c> at the top of each loop iteration. Awaits
    ///     the next <see cref="EnterSlumber" /> signal, marks the description worker active, and
    ///     returns the slumber CT for the new cycle.
    /// </summary>
    public async Task<CancellationToken> WaitForSlumberAsync(CancellationToken stoppingToken)
    {
        Task started;
        lock(_gate) started = _slumberStarted.Task;

        await started.WaitAsync(stoppingToken);

        CancellationToken slumberCt;
        lock(_gate)
        {
            _descriptionActive = true;
            slumberCt          = _slumberCts.Token;
        }

        return slumberCt;
    }

    /// <summary>
    ///     Called by <c>DescriptionTranslationWorker</c> in its <c>finally</c> block once it has
    ///     finished the in-progress translation and is yielding back. Releases the translation
    ///     worker waiting in <see cref="ExitSlumberAsync" />.
    /// </summary>
    public void NotifyDescriptionStopped()
    {
        lock(_gate)
        {
            if(!_descriptionActive) return;

            _descriptionActive = false;
            _descriptionStopped.TrySetResult();
        }

        logger.LogDebug("TranslationPhaseCoordinator: description worker reported stopped.");
    }
}
