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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;

namespace Marechai.ViewModels
{
    public class PersonViewModel : BaseViewModel<int>
    {
        public string    Name             { get; set; }
        public string    Surname          { get; set; }
        public string    CountryOfBirth   { get; set; }
        public DateTime  BirthDate        { get; set; }
        public DateTime? DeathDate        { get; set; }
        public string    Webpage          { get; set; }
        public string    Twitter          { get; set; }
        public string    Facebook         { get; set; }
        public Guid      Photo            { get; set; }
        public string    Alias            { get; set; }
        public string    DisplayName      { get; set; }
        public short?    CountryOfBirthId { get; set; }

        public string FullName => DisplayName ?? Alias ?? $"{Name} {Surname}";
    }
}