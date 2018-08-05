/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Processor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Processor model
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using Cicm.Database.Schemas;

namespace cicm_web.Models
{
    public class Processor
    {
        public int                       AddressBus;
        public Company                   Company;
        public int                       Cores;
        public int                       DataBus;
        public float                     DieSize;
        public int                       Fpr;
        public int                       FprSize;
        public int                       Gpr;
        public int                       GprSize;
        public int                       Id;
        public InstructionSet            InstructionSet;
        public InstructionSetExtension[] InstructionSetExtensions;
        public DateTime                  Introduced;
        public float                     L1Data;
        public float                     L1Instruction;
        public float                     L2;
        public float                     L3;
        public string                    ModelCode;
        public string                    Name;
        public string                    Package;
        public string                    Process;
        public float                     ProcessNm;
        public int                       Simd;
        public int                       SimdSize;
        public double                    Speed;
        public int                       ThreadsPerCore;
        public long                     Transistors;

        public static Processor GetItem(int id)
        {
            Cicm.Database.Schemas.Processor dbItem = Program.Database?.Operations.GetProcessor(id);

            return dbItem == null
                       ? null
                       : new Processor
                       {
                           AddressBus               = dbItem.AddressBus,
                           Name                     = dbItem.Name,
                           Company                  = Company.GetItem(dbItem.Company.Id),
                           Cores                    = dbItem.Cores,
                           DataBus                  = dbItem.DataBus,
                           DieSize                  = dbItem.DieSize,
                           Fpr                      = dbItem.Fpr,
                           FprSize                  = dbItem.FprSize,
                           Gpr                      = dbItem.Gpr,
                           GprSize                  = dbItem.GprSize,
                           InstructionSet           = dbItem.InstructionSet,
                           InstructionSetExtensions = dbItem.InstructionSetExtensions,
                           Introduced               = dbItem.Introduced,
                           L1Data                   = dbItem.L1Data,
                           L1Instruction            = dbItem.L1Instruction,
                           L2                       = dbItem.L2,
                           L3                       = dbItem.L3,
                           ModelCode                = dbItem.ModelCode,
                           Package                  = dbItem.Package,
                           Process                  = dbItem.Process,
                           ProcessNm                = dbItem.ProcessNm,
                           Simd                     = dbItem.Simd,
                           SimdSize                 = dbItem.SimdSize,
                           Speed                    = dbItem.Speed,
                           ThreadsPerCore           = dbItem.ThreadsPerCore,
                           Transistors              = dbItem.Transistors,
                           Id                       = dbItem.Id
                       };
        }

        public static Processor[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Processor> dbItems = null;
            bool?                                 result  = Program.Database?.Operations.GetProcessors(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<Processor> items = new List<Processor>();

            foreach(Cicm.Database.Schemas.Processor dbItem in dbItems)
                items.Add(new Processor
                {
                    AddressBus               = dbItem.AddressBus,
                    Name                     = dbItem.Name,
                    Company                  = Company.GetItem(dbItem.Company.Id),
                    Cores                    = dbItem.Cores,
                    DataBus                  = dbItem.DataBus,
                    DieSize                  = dbItem.DieSize,
                    Fpr                      = dbItem.Fpr,
                    FprSize                  = dbItem.FprSize,
                    Gpr                      = dbItem.Gpr,
                    GprSize                  = dbItem.GprSize,
                    InstructionSet           = dbItem.InstructionSet,
                    InstructionSetExtensions = dbItem.InstructionSetExtensions,
                    Introduced               = dbItem.Introduced,
                    L1Data                   = dbItem.L1Data,
                    L1Instruction            = dbItem.L1Instruction,
                    L2                       = dbItem.L2,
                    L3                       = dbItem.L3,
                    ModelCode                = dbItem.ModelCode,
                    Package                  = dbItem.Package,
                    Process                  = dbItem.Process,
                    ProcessNm                = dbItem.ProcessNm,
                    Simd                     = dbItem.Simd,
                    SimdSize                 = dbItem.SimdSize,
                    Speed                    = dbItem.Speed,
                    ThreadsPerCore           = dbItem.ThreadsPerCore,
                    Transistors              = dbItem.Transistors,
                    Id                       = dbItem.Id
                });

            return items.ToArray();
        }
    }
}