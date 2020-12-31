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
    public class SoftwareVersionViewModel : BaseViewModel<ulong>
    {
        public string    Family     { get; set; }
        public string    Name       { get; set; }
        public string    Codename   { get; set; }
        public string    Version    { get; set; }
        public DateTime? Introduced { get; set; }
        public string    Previous   { get; set; }
        public string    License    { get; set; }
        public ulong     FamilyId   { get; set; }
        public int?      LicenseId  { get; set; }
        public ulong?    PreviousId { get; set; }
    }
}