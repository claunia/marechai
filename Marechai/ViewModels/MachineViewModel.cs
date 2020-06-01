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
using System.Collections.Generic;
using Marechai.Database;

namespace Marechai.ViewModels
{
    public class MachineViewModel : BaseViewModel<int>
    {
        public string                    Name              { get; set; }
        public string                    Model             { get; set; }
        public int                       CompanyId         { get; set; }
        public Guid?                     CompanyLogo       { get; set; }
        public DateTime?                 Introduced        { get; set; }
        public int?                      FamilyId          { get; set; }
        public string                    FamilyName        { get; set; }
        public List<GpuViewModel>        Gpus              { get; set; }
        public List<MemoryViewModel>     Memory            { get; set; }
        public List<ProcessorViewModel>  Processors        { get; set; }
        public List<SoundSynthViewModel> SoundSynthesizers { get; set; }
        public List<StorageViewModel>    Storage           { get; set; }
        public string                    Company           { get; set; }
        public MachineType               Type              { get; set; }
        public string                    Family            { get; set; }
        public string IntroducedView =>
            Introduced?.Year == 1000 ? "Prototype" : Introduced?.ToShortDateString() ?? "Unknown";
    }
}