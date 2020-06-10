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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;

namespace Marechai.Database.Schemas
{
    public static class HexStringToBytesConverter
    {
        public static byte[] StringToHex(string v)
        {
            byte[] hex = new byte[v.Length / 2];
            string str = v.ToLowerInvariant();

            for(int i = 0; i < hex.Length; i++)
            {
                char c0 = str[i * 2];
                char c1 = str[(i * 2) + 1];

                if(c0 >= 0x30 &&
                   c0 <= 0x39)
                    hex[i] += (byte)((c0 - 0x30) * 16);
                else if(c0 >= 0x61 &&
                        c0 <= 0x66)
                    hex[i] += (byte)((c0 - 0x57) * 16);
                else
                    throw new ArgumentOutOfRangeException();

                if(c1 >= 0x30 &&
                   c1 <= 0x39)
                    hex[i] += (byte)(c1 - 0x30);
                else if(c1 >= 0x61 &&
                        c1 <= 0x66)
                    hex[i] += (byte)(c1 - 0x57);
                else
                    throw new ArgumentOutOfRangeException();
            }

            return hex;
        }

        public static string HexToString(byte[] v)
        {
            char[] chars = new char[v.Length * 2];

            for(int i = 0; i < v.Length; i++)
            {
                int c0 = v[i] / 0x10;
                int c1 = v[i] & 0xF;

                if(c0 >= 10)
                    chars[i * 2] = (char)(c0 + 0x57);
                else
                    chars[i * 2] = (char)(c0 + 0x30);

                if(c1 >= 10)
                    chars[(i * 2) + 1] = (char)(c1 + 0x57);
                else
                    chars[(i * 2) + 1] = (char)(c1 + 0x30);
            }

            return new string(chars);
        }
    }
}