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

using System.Security.Cryptography;
using System.Text;

namespace Marechai.Server.Services;

/// <summary>
///     Generates fresh invitation codes in the canonical <c>XXXX-YYYY</c> format using a 31-character
///     unambiguous alphabet (no <c>0</c>/<c>1</c>/<c>I</c>/<c>L</c>/<c>O</c>) and the OS cryptographic RNG.
///     A code carries ~40 bits of entropy (8 chars × log2(31)); bias-free even on a non-power-of-two alphabet
///     because <see cref="RandomNumberGenerator.GetInt32(int, int)" /> rejects-and-retries internally.
/// </summary>
public sealed class InvitationCodeGenerator
{
    /// <summary>
    ///     Unambiguous 31-character alphabet: A–Z minus I/L/O (23 chars) plus 2–9 (8 chars). 0 and 1 are excluded
    ///     because they look like O and I when handwritten or printed in some fonts.
    /// </summary>
    const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public string Generate()
    {
        var sb = new StringBuilder(9);

        for(int i = 0; i < 8; i++)
        {
            if(i == 4) sb.Append('-');
            sb.Append(Alphabet[RandomNumberGenerator.GetInt32(0, Alphabet.Length)]);
        }

        return sb.ToString();
    }
}
