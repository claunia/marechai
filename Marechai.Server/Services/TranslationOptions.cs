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

namespace Marechai.Server.Services;

/// <summary>
///     Server-only background translation settings. <see cref="MaxParallelTranslations" /> controls
///     how many OpenAI/NLLB requests may be in flight concurrently across each background worker
///     wave: <c>0</c> pauses background translations, <c>1</c> preserves the historical serial
///     behavior, and values greater than <c>1</c> allow bounded parallelism.
/// </summary>
public sealed class TranslationOptions
{
    public int MaxParallelTranslations { get; set; } = 1;
}
