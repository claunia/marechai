/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Resolution.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage Resolutions.
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
using System.Data;
using Cicm.Database.Schemas;

namespace Cicm.Database
{
    public partial class Operations
    {
        /// <summary>
        ///     Gets all Resolutions
        /// </summary>
        /// <param name="entries">All Resolutions</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetResolutions(out List<Resolution> entries, int gpuId)
        {
            #if DEBUG
            Console.WriteLine("Getting all resolutions for GPU {0}...", gpuId);
            #endif

            try
            {
                string sql = $"SELECT * FROM resolutions_by_gpu WHERE gpu = {gpuId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = new List<Resolution>();
                foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                    entries.Add(GetResolution((int)dataRow["resolution"]));

                if(entries.Count != 0) return true;

                entries = null;
                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting Resolutions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }
    }
}