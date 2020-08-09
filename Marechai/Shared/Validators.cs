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
using System.Text.RegularExpressions;
using Blazorise;
using Match = System.Text.RegularExpressions.Match;

namespace Marechai.Shared
{
    public static class Validators
    {
        const string _urlRegex =
            @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";

        public static void ValidateString(ValidatorEventArgs e, string message, int maxLength)
        {
            string item = e.Value as string;

            if(item?.Length > maxLength)
            {
                e.ErrorText = message;
                e.Status    = ValidationStatus.Error;
            }
            else
                e.Status = string.IsNullOrWhiteSpace(item) ? ValidationStatus.Error : ValidationStatus.Success;
        }

        public static void ValidateIntroducedDate(ValidatorEventArgs e)
        {
            if(!(e.Value is DateTime item) ||
               item.Year < 1900            ||
               item.Date >= DateTime.UtcNow.Date)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateDouble(ValidatorEventArgs e, double minValue = 0, double maxValue = double.MaxValue)
        {
            if(!(e.Value is double item) ||
               item < minValue           ||
               item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateInteger(ValidatorEventArgs e, int minValue = 0, int maxValue = int.MaxValue)
        {
            if(!(e.Value is int item) ||
               item < minValue        ||
               item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateLong(ValidatorEventArgs e, long minValue = 0, long maxValue = long.MaxValue)
        {
            if(!(e.Value is long item) ||
               item < minValue         ||
               item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateFloat(ValidatorEventArgs e, float minValue = 0, float maxValue = float.MaxValue)
        {
            if(!(e.Value is float item) ||
               item < minValue          ||
               item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateUrl(ValidatorEventArgs e, string message, int maxLength)
        {
            if(!(e.Value is string url))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            if(url.Length < 1 ||
               url.Length > maxLength)
            {
                e.ErrorText = message;
                e.Status    = ValidationStatus.Error;

                return;
            }

            var   rx = new Regex(_urlRegex);
            Match m  = rx.Match(url);

            if(m.Success)
                return;

            e.Status = ValidationStatus.Error;
        }

        public static void ValidateDate(ValidatorEventArgs e)
        {
            if(!(e.Value is DateTime item) ||
               item.Date >= DateTime.UtcNow.Date)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateShort(ValidatorEventArgs e, short minValue = 0, short maxValue = short.MaxValue)
        {
            if(!(e.Value is short item) ||
               item < minValue          ||
               item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateIsbn(ValidatorEventArgs e)
        {
            e.Status = ValidationStatus.Error;

            if(!(e.Value is string isbn))
                return;

            if(isbn.Length != 10 &&
               isbn.Length != 13)
                return;

            for(int c = 0; c < (isbn.Length == 13 ? 13 : 9); c++)
            {
                if(isbn[c] < 0x30 ||
                   isbn[c] > 0x39)
                    return;
            }

            int sum;
            int modulo;

            if(isbn.Length == 10)
            {
                if((isbn[9] < 0x30 || isbn[9] > 0x39) &&
                   isbn[9] != 'x'                     &&
                   isbn[9] != 'X')
                    return;

                sum    = 0;
                modulo = 0;

                for(int i = 0; i < 10; i++)
                {
                    modulo += isbn[i] - 0x30;
                    sum    += modulo;
                }

                modulo = sum % 11;

                if((isbn[9] == 'x' || isbn[9] == 'X') &&
                   modulo == 10)
                    e.Status = ValidationStatus.Success;
                else if(modulo == isbn[9] - 0x30)
                    e.Status = ValidationStatus.Success;

                return;
            }

            if(isbn[0] != '9' ||
               isbn[1] != '7' ||
               (isbn[1] != '8' && isbn[1] != '9'))
                return;

            sum = (isbn[0] - 0x30) + ((isbn[1] - 0x30) * 3) + (isbn[2]  - 0x30) + ((isbn[3]  - 0x30) * 3) +
                  (isbn[4] - 0x30) + ((isbn[5] - 0x30) * 3) + (isbn[6]  - 0x30) + ((isbn[7]  - 0x30) * 3) +
                  (isbn[8] - 0x30) + ((isbn[9] - 0x30) * 3) + (isbn[10] - 0x30) + ((isbn[11] - 0x30) * 3);

            modulo = sum % 10;

            if(modulo != 0)
                modulo = 10 - modulo;

            if(modulo == isbn[12] - 0x30)
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateIssn(ValidatorEventArgs e)
        {
            e.Status = ValidationStatus.Error;

            if(!(e.Value is string issn))
                return;

            if(issn.Length != 8)
                return;

            for(int c = 0; c < 7; c++)
            {
                if(issn[c] < 0x30 ||
                   issn[c] > 0x39)
                    return;
            }

            if((issn[7] < 0x30 || issn[7] > 0x39) &&
               issn[7] != 'x'                     &&
               issn[7] != 'X')
                return;

            int sum    = 0;
            int modulo = 0;

            for(int i = 0; i < 7; i++)
            {
                modulo += issn[i] - 0x30;
                sum    += modulo;
            }

            modulo = sum % 11;

            if((issn[7] == 'x' || issn[7] == 'X') &&
               modulo == 10)
                e.Status = ValidationStatus.Success;
            else if(modulo == issn[7] - 0x30)
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateUnsignedLong(ValidatorEventArgs e, ulong minValue = 0,
                                                ulong maxValue = long.MaxValue)
        {
            if(!(e.Value is ulong item) ||
               item < minValue          ||
               item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }
    }
}