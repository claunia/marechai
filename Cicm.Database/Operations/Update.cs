/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Update.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to update the database.
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
using System.IO;

namespace Cicm.Database
{
    public partial class Operations
    {
        /// <summary>
        ///     Updates opened database to last known version
        /// </summary>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateDatabase()
        {
            bool dbV2             = !dbCore.TableExists("cicm_db");
            int  currentDbVersion = 2;

            if(!dbV2)
            {
                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = "SELECT * FROM cicm_db";
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                {
                    int newId = int.Parse(dataRow["version"].ToString());

                    if(newId > currentDbVersion) currentDbVersion = newId;
                }
            }

            Console.WriteLine("Database version: {0}", currentDbVersion);

            if(currentDbVersion > DB_VERSION)
            {
                Console.WriteLine("Current database version is higher than last supported version {0}, cannot continue...",
                                  DB_VERSION);
                return false;
            }

            if(currentDbVersion == DB_VERSION) return true;

            for(int i = currentDbVersion; i < DB_VERSION; i++)
                switch(i)
                {
                    case 2:
                    {
                        UpdateDatabaseV2ToV3();
                        break;
                    }
                }

            OptimizeDatabase();
            return true;
        }

        void UpdateDatabaseV2ToV3()
        {
            Console.WriteLine("Updating database to version 3");

            Console.WriteLine("Creating versioning table");
            IDbCommand dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText = @"CREATE TABLE `cicm_db` (
                                             `id` INT NOT NULL AUTO_INCREMENT,
                                             `version` INT NOT NULL,
                                             `updated` DATETIME DEFAULT CURRENT_TIMESTAMP,
                                             PRIMARY KEY (`id`) )";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming table `admin` to `admins`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `admin` RENAME TO `admins`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `browser_test.idstring` to `browser_test.user_agent`");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText =
                @"ALTER TABLE `browser_test` CHANGE COLUMN `idstring` `user_agent` varchar(128) NOT NULL DEFAULT '';";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming table `browser_test` to `browser_tests`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `browser_test` RENAME TO `browser_tests`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `Companias.Compania` to `Companias.name`");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText =
                @"ALTER TABLE `Companias` CHANGE COLUMN `Compania` `name` varchar(128) NOT NULL DEFAULT '';";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming table `Companias` to `companies`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `Companias` RENAME TO `companies`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `computers.spu` to `computers.sound_synth`");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText =
                @"ALTER TABLE `computers` CHANGE COLUMN `spu` `sound_synth` int(11) NOT NULL DEFAULT '0'";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming column `computers.mpu` to `music_synth.name`");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText =
                @"ALTER TABLE `computers` CHANGE COLUMN `mpu` `music_synth` int(11) NOT NULL DEFAULT '0'";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Dropping column `computers.comment`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `computers` DROP COLUMN `comment`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `consoles.name` to `consoles.model`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `consoles` CHANGE COLUMN `name` `model` char(50) NOT NULL DEFAULT ''";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming column `consoles.spu` to `consoles.sound_synth`");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText =
                @"ALTER TABLE `consoles` CHANGE COLUMN `spu` `sound_synth` int(11) NOT NULL DEFAULT '0'";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming column `consoles.mpu` to `consoles.music_synth`");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText =
                @"ALTER TABLE `consoles` CHANGE COLUMN `mpu` `music_synth` int(11) NOT NULL DEFAULT '0'";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Dropping column `consoles.comments`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `consoles` DROP COLUMN `comments`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `cpu.cpu` to `cpu.name`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `cpu` CHANGE COLUMN `cpu` `name` char(50) NOT NULL DEFAULT ''";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming table `cpu` to `processors`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `cpu` RENAME TO `processors`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `DSPs.DSP` to `DSPs.name`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `DSPs` CHANGE COLUMN `DSP` `name` char(50) NOT NULL DEFAULT ''";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming table `DSPs` to `sound_synths`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `DSPs` RENAME TO `sound_synths`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `Formatos_de_disco.Format` to `Formatos_de_disco.description`");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText =
                @"ALTER TABLE `Formatos_de_disco` CHANGE COLUMN `Format` `description` char(50) NOT NULL DEFAULT ''";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming table `Formatos_de_disco` to `disk_formats`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `Formatos_de_disco` RENAME TO `disk_formats`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `gpus.gpu` to `gpus.name`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `gpus` CHANGE COLUMN `gpu` `name` char(128) NOT NULL DEFAULT ''";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming table `money_donation` to `money_donations`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `money_donation` RENAME TO `money_donations`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming column `mpus.mpu` to `mpus.name`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `mpus` CHANGE COLUMN `mpu` `name` char(50) NOT NULL DEFAULT ''";
            dbCmd.ExecuteNonQuery();
            Console.WriteLine("Renaming table `mpus` to `music_synths`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `mpus` RENAME TO `music_synths`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming table `own_computer` to `owned_computers`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `own_computer` RENAME TO `owned_computers`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Renaming table `own_consoles` to `owned_consoles`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"ALTER TABLE `own_consoles` RENAME TO `owned_consoles`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Dropping table `procesadores_principales`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"DROP TABLE `procesadores_principales`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Getting all items from `console_company`");

            Dictionary<int, string> consoleCompanies = new Dictionary<int, string>();
            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from console_company";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                consoleCompanies.Add(int.Parse(dataRow["id"].ToString()), dataRow["company"].ToString());

            Dictionary<int, int> conversionEquivalents = new Dictionary<int, int>();
            IDbTransaction       trans;

            Console.WriteLine("Converting all items from `console_company` to `companies`");
            foreach(KeyValuePair<int, string> consoleCompany in consoleCompanies)
            {
                dbCmd                     = dbCon.CreateCommand();
                dataAdapter               = dbCore.GetNewDataAdapter();
                dbCmd.CommandText         = $"SELECT * from companies WHERE name LIKE '{consoleCompany.Value}'";
                dataSet                   = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                if(dataSet.Tables[0].Rows.Count == 1)
                {
                    Console.WriteLine("Converting console company `{0}` to company `{1}`", consoleCompany.Value,
                                      dataSet.Tables[0].Rows[0]["name"]);
                    conversionEquivalents.Add(consoleCompany.Key,
                                              int.Parse(dataSet.Tables[0].Rows[0]["id"].ToString()));
                }
                else
                {
                    Console.Write("Adding new company `{0}`... ", consoleCompany.Value);
                    dbCmd             = dbCon.CreateCommand();
                    trans             = dbCon.BeginTransaction();
                    dbCmd.Transaction = trans;
                    dbCmd.CommandText = $"INSERT INTO companies (name) VALUES ('{consoleCompany.Value}')";

                    dbCmd.ExecuteNonQuery();
                    trans.Commit();
                    dbCmd.Dispose();

                    long id = dbCore.LastInsertRowId;
                    Console.WriteLine("got id {0}", id);
                    conversionEquivalents.Add(consoleCompany.Key, (int)id);
                }
            }

            Console.WriteLine("Getting all items from `consoles`");
            Dictionary<int, int> consoleIdAndCompanyId = new Dictionary<int, int>();
            dbCmd                     = dbCon.CreateCommand();
            dataAdapter               = dbCore.GetNewDataAdapter();
            dbCmd.CommandText         = "SELECT id,company from consoles";
            dataSet                   = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);
            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                consoleIdAndCompanyId.Add(int.Parse(dataRow["id"].ToString()),
                                          int.Parse(dataRow["company"].ToString()));

            trans = dbCon.BeginTransaction();
            foreach(KeyValuePair<int, int> keyValuePair in consoleIdAndCompanyId)
            {
                conversionEquivalents.TryGetValue(keyValuePair.Value, out int newId);
                Console.WriteLine("Converting console company {0} to company {1} for console {2}... ",
                                  keyValuePair.Value, newId, keyValuePair.Key);
                dbCmd             = dbCon.CreateCommand();
                dbCmd.Transaction = trans;
                dbCmd.CommandText = $"UPDATE consoles SET company = {newId} WHERE id = {keyValuePair.Key}";
                dbCmd.ExecuteNonQuery();
                dbCmd.Dispose();
            }

            Console.WriteLine("Comitting changes...");
            trans.Commit();

            Console.WriteLine("Moving company logos...");
            foreach(string file in Directory.GetFiles("wwwroot/assets/logos/computers/", "*",
                                                      SearchOption.TopDirectoryOnly))
            {
                string newPath = Path.Combine("wwwroot/assets/logos/", Path.GetFileName(file));
                Console.WriteLine("Moving {0} to {1}...", file, newPath);
                File.Move(file, newPath);
            }

            Console.WriteLine("Removing old computer company logos directory...");
            Directory.Delete("wwwroot/assets/logos/computers");

            Console.WriteLine("Moving console company logos...");
            foreach(string file in Directory.GetFiles("wwwroot/assets/logos/consoles/", "*",
                                                      SearchOption.TopDirectoryOnly))
            {
                string oldNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if(!int.TryParse(oldNameWithoutExtension, out int oldId))
                {
                    Console.WriteLine("Removing stray file {0}...", file);
                    File.Delete(file);
                    continue;
                }

                conversionEquivalents.TryGetValue(oldId, out int newId);
                string extension = Path.GetExtension(file);

                string newPath = Path.Combine("wwwroot/assets/logos/", $"{newId}{extension}");
                if(File.Exists(newPath))
                {
                    Console.WriteLine("Removing duplicate file {0}...", file);
                    File.Delete(file);
                }
                else
                {
                    Console.WriteLine("Moving {0} to {1}...", file, newPath);
                    File.Move(file, newPath);
                }
            }

            Console.WriteLine("Removing old console company logos directory...");
            Directory.Delete("wwwroot/assets/logos/consoles");

            Console.WriteLine("Dropping table `console_company`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = @"DROP TABLE `console_company`;";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Setting new database version to 3...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = $"INSERT INTO cicm_db (version) VALUES ('3')";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Finished update version to 3...");
        }

        void OptimizeDatabase()
        {
            IDbCommand     dbCmd       = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SHOW TABLES";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                string tableName = dataRow[0].ToString();

                Console.WriteLine("Optimizing table `{0}`", tableName);
                dbCmd             = dbCon.CreateCommand();
                dataAdapter       = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = $"OPTIMIZE TABLE '{tableName}'";
            }
        }
    }
}