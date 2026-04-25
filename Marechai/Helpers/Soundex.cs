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

namespace Marechai.Helpers;

public static class Soundex
{
    static readonly char[] _map =
    [
    //  A    B    C    D    E    F    G    H    I    J    K    L    M
        '0', '1', '2', '3', '0', '1', '2', '0', '0', '2', '2', '4', '5',
    //  N    O    P    Q    R    S    T    U    V    W    X    Y    Z
        '5', '0', '1', '2', '6', '2', '3', '0', '1', '0', '2', '0', '2'
    ];

    public static string Generate(string? input)
    {
        if(string.IsNullOrWhiteSpace(input))
            return "0000";

        Span<char> result = stackalloc char[4];
        result[0] = char.ToUpperInvariant(input[0]);
        int resultIndex = 1;
        char lastCode = GetCode(result[0]);

        for(int i = 1; i < input.Length && resultIndex < 4; i++)
        {
            char c = input[i];

            if(!char.IsLetter(c))
                continue;

            char code = GetCode(char.ToUpperInvariant(c));

            if(code == '0' || code == lastCode)
            {
                lastCode = code;

                continue;
            }

            result[resultIndex++] = code;
            lastCode              = code;
        }

        while(resultIndex < 4)
            result[resultIndex++] = '0';

        return new string(result);
    }

    static char GetCode(char c)
    {
        int index = char.ToUpperInvariant(c) - 'A';

        return index is >= 0 and < 26 ? _map[index] : '0';
    }
}
