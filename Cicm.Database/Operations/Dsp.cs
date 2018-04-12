/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Dsp.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage DSPs.
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
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetDsps(out List<Dsp> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all DSPs...");
            #endif

            try
            {
                const string SQL = "SELECT * from DSPs";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = DspsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting DSPs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetDsps(out List<Dsp> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} DSPs from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM DSPs LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = DspsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting DSPs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public Dsp GetDsp(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting DSP with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from DSPs WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Dsp> entries = DspsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting DSP.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountDsps()
        {
            #if DEBUG
            Console.WriteLine("Counting DSPs...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM DSPs";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddDsp(Dsp entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding DSP `{0}`...", entry.DSP);
            #endif

            IDbCommand     dbcmd = GetCommandDsp(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO DSPs (DSP)" + " VALUES (@DSP)";

            dbcmd.CommandText = SQL;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            id = dbCore.LastInsertRowId;

            #if DEBUG
            Console.WriteLine(" id {0}", id);
            #endif

            return true;
        }

        public bool UpdateDsp(Dsp entry)
        {
            #if DEBUG
            Console.WriteLine("Updating DSP `{0}`...", entry.DSP);
            #endif

            IDbCommand     dbcmd = GetCommandDsp(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE DSPs SET DSP = @DSP " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveDsp(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing DSP widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM DSPs WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandDsp(Dsp entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@DSP";

            param1.DbType = DbType.String;

            param1.Value = entry.DSP;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<Dsp> DspsFromDataTable(DataTable dataTable)
        {
            List<Dsp> entries = new List<Dsp>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Dsp entry = new Dsp {id = int.Parse(dataRow["id"].ToString()), DSP = dataRow["DSP"].ToString()};

                entries.Add(entry);
            }

            return entries;
        }
    }
}