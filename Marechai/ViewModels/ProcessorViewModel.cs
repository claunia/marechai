﻿/******************************************************************************
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
using System.Collections.Generic;

namespace Marechai.ViewModels
{
    public class ProcessorViewModel : BaseViewModel<int>
    {
        public string       Name                     { get; set; }
        public string       CompanyName              { get; set; }
        public string       ModelCode                { get; set; }
        public DateTime?    Introduced               { get; set; }
        public double?      Speed                    { get; set; }
        public string       Package                  { get; set; }
        public int?         Gprs                     { get; set; }
        public int?         GprSize                  { get; set; }
        public int?         Fprs                     { get; set; }
        public int?         FprSize                  { get; set; }
        public int?         Cores                    { get; set; }
        public int?         ThreadsPerCore           { get; set; }
        public string       Process                  { get; set; }
        public float?       ProcessNm                { get; set; }
        public float?       DieSize                  { get; set; }
        public long?        Transistors              { get; set; }
        public int?         DataBus                  { get; set; }
        public int?         AddrBus                  { get; set; }
        public int?         SimdRegisters            { get; set; }
        public int?         SimdSize                 { get; set; }
        public float?       L1Instruction            { get; set; }
        public float?       L1Data                   { get; set; }
        public float?       L2                       { get; set; }
        public float?       L3                       { get; set; }
        public string       InstructionSet           { get; set; }
        public List<string> InstructionSetExtensions { get; set; }
        public int?         CompanyId                { get; set; }
        public int?         InstructionSetId         { get; set; }

        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}