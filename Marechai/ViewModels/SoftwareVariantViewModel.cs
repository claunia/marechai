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
using Marechai.Database;

namespace Marechai.ViewModels
{
    public class SoftwareVariantViewModel : BaseViewModel<ulong>
    {
        public string           Name              { get; set; }
        public string           Version           { get; set; }
        public DateTime?        Introduced        { get; set; }
        public ulong?           ParentId          { get; set; }
        public string           Parent            { get; set; }
        public ulong            SoftwareVersionId { get; set; }
        public string           SoftwareVersion   { get; set; }
        public ulong?           MinimumMemory     { get; set; }
        public ulong?           RecommendedMemory { get; set; }
        public ulong?           RequiredStorage   { get; set; }
        public string           PartNumber        { get; set; }
        public string           SerialNumber      { get; set; }
        public string           ProductCode       { get; set; }
        public string           CatalogueNumber   { get; set; }
        public DistributionMode DistributionMode  { get; set; }
        public string           Family            { get; set; }
    }
}