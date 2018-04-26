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
using Cicm.Database.Schemas.Sql;

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
                        UpdateDatabaseToV3();
                        break;
                    }
                    case 3:
                    {
                        UpdateDatabaseToV4();
                        break;
                    }
                    case 4:
                    {
                        UpdateDatabaseToV5();
                        break;
                    }
                    case 5:
                    {
                        UpdateDatabaseToV6();
                        break;
                    }
                    case 6:
                    {
                        UpdateDatabaseToV7();
                        break;
                    }
                    case 7:
                    {
                        UpdateDatabaseToV8();
                        break;
                    }
                    case 8:
                    {
                        UpdateDatabaseToV9();
                        break;
                    }
                    case 9:
                    {
                        UpdateDatabaseToV10();
                        break;
                    }
                    case 10:
                    {
                        UpdateDatabaseToV11();
                        break;
                    }
                }

            OptimizeDatabase();
            return true;
        }

        void UpdateDatabaseToV3()
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
            dbCmd.CommandText = @"INSERT INTO cicm_db (version) VALUES ('3')";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Finished update version to 3...");
        }

        void UpdateDatabaseToV4()
        {
            Console.WriteLine("Updating database to version 4");
            IDbCommand     dbCmd;
            IDbTransaction trans;

            if(dbCore is Mysql)
            {
                Console.WriteLine("Changing table formats...");
                dbCmd             = dbCon.CreateCommand();
                trans             = dbCon.BeginTransaction();
                dbCmd.Transaction = trans;
                dbCmd.CommandText = "ALTER TABLE `admins` ROW_FORMAT = DYNAMIC;\n"          +
                                    "ALTER TABLE `browser_tests` ROW_FORMAT = DYNAMIC;\n"   +
                                    "ALTER TABLE `cicm_db` ROW_FORMAT = DYNAMIC;\n"         +
                                    "ALTER TABLE `companies` ROW_FORMAT = DYNAMIC;\n"       +
                                    "ALTER TABLE `computers` ROW_FORMAT = DYNAMIC;\n"       +
                                    "ALTER TABLE `consoles` ROW_FORMAT = DYNAMIC;\n"        +
                                    "ALTER TABLE `disk_formats` ROW_FORMAT = DYNAMIC;\n"    +
                                    "ALTER TABLE `forbidden` ROW_FORMAT = DYNAMIC;\n"       +
                                    "ALTER TABLE `gpus` ROW_FORMAT = DYNAMIC;\n"            +
                                    "ALTER TABLE `log` ROW_FORMAT = DYNAMIC;\n"             +
                                    "ALTER TABLE `money_donations` ROW_FORMAT = DYNAMIC;\n" +
                                    "ALTER TABLE `music_synths` ROW_FORMAT = DYNAMIC;\n"    +
                                    "ALTER TABLE `news` ROW_FORMAT = DYNAMIC;\n"            +
                                    "ALTER TABLE `owned_computers` ROW_FORMAT = DYNAMIC;\n" +
                                    "ALTER TABLE `owned_consoles` ROW_FORMAT = DYNAMIC;\n"  +
                                    "ALTER TABLE `processors` ROW_FORMAT = DYNAMIC;\n"      +
                                    "ALTER TABLE `sound_synths` ROW_FORMAT = DYNAMIC;\n";
                dbCmd.ExecuteNonQuery();
                trans.Commit();
                dbCmd.Dispose();
            }

            Console.WriteLine("Correcting primary key on table `processors`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "ALTER TABLE `processors` ADD PRIMARY KEY (id);\n" + "DROP INDEX id ON processors";
            dbCmd.ExecuteNonQuery();

            Console.WriteLine("Creating indexes on `admins`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_admins_user ON admins (user);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `browser_tests`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_browser_tests_user_agent ON browser_tests (user_agent);\n" +
                                "CREATE INDEX idx_browser_tests_browser ON browser_tests (browser);\n"       +
                                "CREATE INDEX idx_browser_tests_version ON browser_tests (version);\n"       +
                                "CREATE INDEX idx_browser_tests_os ON browser_tests (os);\n"                 +
                                "CREATE INDEX idx_browser_tests_platform ON browser_tests (platform);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `companies`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_companies_name ON companies (name);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `computers`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_computers_company ON computers (company);\n"         +
                                "CREATE INDEX idx_computers_year ON computers (year);\n"               +
                                "CREATE INDEX idx_computers_model ON computers (model);\n"             +
                                "CREATE INDEX idx_computers_cpu1 ON computers (cpu1);\n"               +
                                "CREATE INDEX idx_computers_cpu2 ON computers (cpu2);\n"               +
                                "CREATE INDEX idx_computers_mhz1 ON computers (mhz1);\n"               +
                                "CREATE INDEX idx_computers_mhz2 ON computers (mhz2);\n"               +
                                "CREATE INDEX idx_computers_bits ON computers (bits);\n"               +
                                "CREATE INDEX idx_computers_ram ON computers (ram);\n"                 +
                                "CREATE INDEX idx_computers_rom ON computers (rom);\n"                 +
                                "CREATE INDEX idx_computers_gpu ON computers (gpu);\n"                 +
                                "CREATE INDEX idx_computers_vram ON computers (vram);\n"               +
                                "CREATE INDEX idx_computers_colors ON computers (colors);\n"           +
                                "CREATE INDEX idx_computers_res ON computers (res);\n"                 +
                                "CREATE INDEX idx_computers_sound_synth ON computers (sound_synth);\n" +
                                "CREATE INDEX idx_computers_music_synth ON computers (music_synth);\n" +
                                "CREATE INDEX idx_computers_hdd1 ON computers (hdd1);\n"               +
                                "CREATE INDEX idx_computers_hdd2 ON computers (hdd2);\n"               +
                                "CREATE INDEX idx_computers_hdd3 ON computers (hdd3);\n"               +
                                "CREATE INDEX idx_computers_disk1 ON computers (disk1);\n"             +
                                "CREATE INDEX idx_computers_disk2 ON computers (disk2);\n"             +
                                "CREATE INDEX idx_computers_cap1 ON computers (cap1);\n"               +
                                "CREATE INDEX idx_computers_cap2 ON computers (cap2);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `consoles`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_consoles_company ON consoles (company);\n"         +
                                "CREATE INDEX idx_consoles_year ON consoles (year);\n"               +
                                "CREATE INDEX idx_consoles_model ON consoles (model);\n"             +
                                "CREATE INDEX idx_consoles_cpu1 ON consoles (cpu1);\n"               +
                                "CREATE INDEX idx_consoles_cpu2 ON consoles (cpu2);\n"               +
                                "CREATE INDEX idx_consoles_mhz1 ON consoles (mhz1);\n"               +
                                "CREATE INDEX idx_consoles_mhz2 ON consoles (mhz2);\n"               +
                                "CREATE INDEX idx_consoles_bits ON consoles (bits);\n"               +
                                "CREATE INDEX idx_consoles_ram ON consoles (ram);\n"                 +
                                "CREATE INDEX idx_consoles_rom ON consoles (rom);\n"                 +
                                "CREATE INDEX idx_consoles_gpu ON consoles (gpu);\n"                 +
                                "CREATE INDEX idx_consoles_vram ON consoles (vram);\n"               +
                                "CREATE INDEX idx_consoles_colors ON consoles (colors);\n"           +
                                "CREATE INDEX idx_consoles_res ON consoles (res);\n"                 +
                                "CREATE INDEX idx_consoles_sound_synth ON consoles (sound_synth);\n" +
                                "CREATE INDEX idx_consoles_music_synth ON consoles (music_synth);\n" +
                                "CREATE INDEX idx_consoles_palette ON consoles (palette);\n"         +
                                "CREATE INDEX idx_consoles_format ON consoles (format);\n"           +
                                "CREATE INDEX idx_consoles_cap ON consoles (cap);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `disk_formats`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_disk_formats_description ON disk_formats (description);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `forbidden`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_forbidden_browser ON forbidden (browser);\n" +
                                "CREATE INDEX idx_forbidden_date ON forbidden (date);\n"       +
                                "CREATE INDEX idx_forbidden_ip ON forbidden (ip);\n"           +
                                "CREATE INDEX idx_forbidden_referer ON forbidden (referer);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `gpus`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_gpus_name ON gpus (name);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `log`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_log_browser ON log (browser);\n" +
                                "CREATE INDEX idx_log_date ON log (date);\n"       +
                                "CREATE INDEX idx_log_ip ON log (ip);\n"           +
                                "CREATE INDEX idx_log_referer ON log (referer);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `money_donations`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_money_donations_donator ON money_donations (donator);\n" +
                                "CREATE INDEX idx_money_donations_quantity ON money_donations (quantity);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `music_synths`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_music_synts_name ON music_synths (name);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `news`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_news_date ON news (date);\n" +
                                "CREATE INDEX idx_news_type ON news (type);\n" +
                                "CREATE INDEX idx_news_ip ON news (added_id);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `owned_computers`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_owned_computers_db_id ON owned_computers (db_id);\n"     +
                                "CREATE INDEX idx_owned_computers_date ON owned_computers (date);\n"       +
                                "CREATE INDEX idx_owned_computers_status ON owned_computers (status);\n"   +
                                "CREATE INDEX idx_owned_computers_trade ON owned_computers (trade);\n"     +
                                "CREATE INDEX idx_owned_computers_boxed ON owned_computers (boxed);\n"     +
                                "CREATE INDEX idx_owned_computers_manuals ON owned_computers (manuals);\n" +
                                "CREATE INDEX idx_owned_computers_cpu1 ON owned_computers (cpu1);\n"       +
                                "CREATE INDEX idx_owned_computers_cpu2 ON owned_computers (cpu2);\n"       +
                                "CREATE INDEX idx_owned_computers_mhz1 ON owned_computers (mhz1);\n"       +
                                "CREATE INDEX idx_owned_computers_mhz2 ON owned_computers (mhz2);\n"       +
                                "CREATE INDEX idx_owned_computers_ram ON owned_computers (ram);\n"         +
                                "CREATE INDEX idx_owned_computers_vram ON owned_computers (vram);\n"       +
                                "CREATE INDEX idx_owned_computers_rigid ON owned_computers (rigid);\n"     +
                                "CREATE INDEX idx_owned_computers_disk1 ON owned_computers (disk1);\n"     +
                                "CREATE INDEX idx_owned_computers_disk2 ON owned_computers (disk2);\n"     +
                                "CREATE INDEX idx_owned_computers_cap1 ON owned_computers (cap1);\n"       +
                                "CREATE INDEX idx_owned_computers_cap2 ON owned_computers (cap2);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `owned_consoles`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_owned_consoles_db_id ON owned_consoles (db_id);\n"    +
                                "CREATE INDEX idx_owned_consoles_date    ON owned_consoles (date);\n"   +
                                "CREATE INDEX idx_owned_consoles_status  ON owned_consoles (status);\n" +
                                "CREATE INDEX idx_owned_consoles_trade   ON owned_consoles (trade);\n"  +
                                "CREATE INDEX idx_owned_consoles_boxed   ON owned_consoles (boxed);\n"  +
                                "CREATE INDEX idx_owned_consoles_manuals ON owned_consoles (manuals);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `processors`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_processors_name ON processors (name);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating indexes on `sound_synths`...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX idx_sound_synths_name ON sound_synths (name);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 4...");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('4')";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV5()
        {
            Console.WriteLine("Updating database to version 5");

            Console.WriteLine("Creating foreign keys for table `computers`");
            IDbCommand dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText = V5.ComputersForeignKeys;
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();

            Console.WriteLine("Creating foreign keys for table `consoles`");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = V5.ConsolesForeignKeys;
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 5...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('5')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV6()
        {
            Console.WriteLine("Updating database to version 6");

            Console.WriteLine("Creating table `iso3166_1_numeric`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V6.Iso3166Numeric;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Filling table `iso3166_1_numeric`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V6.Iso3166NumericValues;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new columns to table `companies`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `companies` ADD COLUMN `founded` DATETIME NULL;\n"        +
                                "ALTER TABLE `companies` ADD COLUMN `website` VARCHAR(255) NULL;\n"    +
                                "ALTER TABLE `companies` ADD COLUMN `twitter` VARCHAR(45) NULL;\n"     +
                                "ALTER TABLE `companies` ADD COLUMN `facebook` VARCHAR(45) NULL;\n"    +
                                "ALTER TABLE `companies` ADD COLUMN `sold` DATETIME NULL;\n"           +
                                "ALTER TABLE `companies` ADD COLUMN `sold_to` INT(11) NULL;\n"         +
                                "ALTER TABLE `companies` ADD COLUMN `address` VARCHAR(80) NULL;\n"     +
                                "ALTER TABLE `companies` ADD COLUMN `city` VARCHAR(80) NULL;\n"        +
                                "ALTER TABLE `companies` ADD COLUMN `province` VARCHAR(80) NULL;\n"    +
                                "ALTER TABLE `companies` ADD COLUMN `postal_code` VARCHAR(25) NULL;\n" +
                                "ALTER TABLE `companies` ADD COLUMN `country` SMALLINT(3) UNSIGNED ZEROFILL NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new indexes to table `companies`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX `idx_companies_founded` ON `companies` (`founded`);\n"         +
                                "CREATE INDEX `idx_companies_website` ON `companies` (`website`);\n"         +
                                "CREATE INDEX `idx_companies_twitter` ON `companies` (`twitter`);\n"         +
                                "CREATE INDEX `idx_companies_facebook` ON `companies` (`facebook`);\n"       +
                                "CREATE INDEX `idx_companies_sold` ON `companies` (`sold`);\n"               +
                                "CREATE INDEX `idx_companies_sold_to` ON `companies` (`sold_to`);\n"         +
                                "CREATE INDEX `idx_companies_address` ON `companies` (`address`);\n"         +
                                "CREATE INDEX `idx_companies_city` ON `companies` (`city`);\n"               +
                                "CREATE INDEX `idx_companies_province` ON `companies` (`province`);\n"       +
                                "CREATE INDEX `idx_companies_postal_code` ON `companies` (`postal_code`);\n" +
                                "CREATE INDEX `idx_companies_country` ON `companies` (`country`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new foreign keys to table `companies`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `companies` ADD FOREIGN KEY `fk_companies_sold_to` (sold_to) REFERENCES `companies` (`id`);\n" +
                "ALTER TABLE `companies` ADD FOREIGN KEY `fk_companies_country` (country) REFERENCES `iso3166_1_numeric` (`id`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 6...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('6')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV7()
        {
            Console.WriteLine("Updating database to version 7");

            Console.WriteLine("Adding new columns to table `companies`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `companies` ADD COLUMN `status` INT NOT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new indexes to table `companies`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX `idx_companies_status` ON `companies` (`status`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 7...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('7')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV8()
        {
            Console.WriteLine("Updating database to version 8");

            Console.WriteLine("Creating table `company_logos`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V8.CompanyLogos;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 8...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('8')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV9()
        {
            Console.WriteLine("Updating database to version 9");

            Console.WriteLine("Creating table `company_descriptions`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V9.CompanyDescriptions;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 9...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('9')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV10()
        {
            Console.WriteLine("Updating database to version 10");

            Console.WriteLine("Creating table `instruction_sets`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V10.InstructionSets;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating table `instruction_set_extensions`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V10.InstructionSetExtensions;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating table `instruction_set_extensions_by_processor`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V10.InstructionSetExtensionsByProcessor;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new columns to table `processors`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `processors` ADD COLUMN `company` INT NULL;\n"            +
                                "ALTER TABLE `processors` ADD COLUMN `model_code` VARCHAR(45) NULL;\n" +
                                "ALTER TABLE `processors` ADD COLUMN `introduced` DATETIME NULL;\n"    +
                                "ALTER TABLE `processors` ADD COLUMN `instruction_set` INT NULL;\n"    +
                                "ALTER TABLE `processors` ADD COLUMN `speed` DOUBLE NULL;\n"           +
                                "ALTER TABLE `processors` ADD COLUMN `package` VARCHAR(45) NULL;\n"    +
                                "ALTER TABLE `processors` ADD COLUMN `GPRs` INT NULL;\n"               +
                                "ALTER TABLE `processors` ADD COLUMN `GPR_size` INT NULL;\n"           +
                                "ALTER TABLE `processors` ADD COLUMN `FPRs` INT NULL;\n"               +
                                "ALTER TABLE `processors` ADD COLUMN `FPR_size` INT NULL;\n"           +
                                "ALTER TABLE `processors` ADD COLUMN `cores` INT NULL;\n"              +
                                "ALTER TABLE `processors` ADD COLUMN `threads_per_core` INT NULL;\n"   +
                                "ALTER TABLE `processors` ADD COLUMN `process` VARCHAR(45) NULL;\n"    +
                                "ALTER TABLE `processors` ADD COLUMN `process_nm` FLOAT NULL;\n"       +
                                "ALTER TABLE `processors` ADD COLUMN `die_size` FLOAT NULL;\n"         +
                                "ALTER TABLE `processors` ADD COLUMN `transistors` BIGINT NULL;\n"     +
                                "ALTER TABLE `processors` ADD COLUMN `data_bus` INT NULL;\n"           +
                                "ALTER TABLE `processors` ADD COLUMN `addr_bus` INT NULL;\n"           +
                                "ALTER TABLE `processors` ADD COLUMN `SIMD_registers` INT NULL;\n"     +
                                "ALTER TABLE `processors` ADD COLUMN `SIMD_size` INT NULL;\n"          +
                                "ALTER TABLE `processors` ADD COLUMN `L1_instruction` FLOAT NULL;\n"   +
                                "ALTER TABLE `processors` ADD COLUMN `L1_data` FLOAT NULL;\n"          +
                                "ALTER TABLE `processors` ADD COLUMN `L2` FLOAT NULL;\n"               +
                                "ALTER TABLE `processors` ADD COLUMN `L3` FLOAT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new indexes to table `processors`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "CREATE INDEX `idx_processors_company` ON `processors` (`company`);\n"                   +
                "CREATE INDEX `idx_processors_model_code` ON `processors` (`model_code`);\n"             +
                "CREATE INDEX `idx_processors_introduced` ON `processors` (`introduced`);\n"             +
                "CREATE INDEX `idx_processors_instruction_set` ON `processors` (`instruction_set`);\n"   +
                "CREATE INDEX `idx_processors_speed` ON `processors` (`speed`);\n"                       +
                "CREATE INDEX `idx_processors_package` ON `processors` (`package`);\n"                   +
                "CREATE INDEX `idx_processors_GPRs` ON `processors` (`GPRs`);\n"                         +
                "CREATE INDEX `idx_processors_GPR_size` ON `processors` (`GPR_size`);\n"                 +
                "CREATE INDEX `idx_processors_FPRs` ON `processors` (`FPRs`);\n"                         +
                "CREATE INDEX `idx_processors_FPR_size` ON `processors` (`FPR_size`);\n"                 +
                "CREATE INDEX `idx_processors_cores` ON `processors` (`cores`);\n"                       +
                "CREATE INDEX `idx_processors_threads_per_core` ON `processors` (`threads_per_core`);\n" +
                "CREATE INDEX `idx_processors_process` ON `processors` (`process`);\n"                   +
                "CREATE INDEX `idx_processors_process_nm` ON `processors` (`process_nm`);\n"             +
                "CREATE INDEX `idx_processors_die_size` ON `processors` (`die_size`);\n"                 +
                "CREATE INDEX `idx_processors_transistors` ON `processors` (`transistors`);\n"           +
                "CREATE INDEX `idx_processors_data_bus` ON `processors` (`data_bus`);\n"                 +
                "CREATE INDEX `idx_processors_addr_bus` ON `processors` (`addr_bus`);\n"                 +
                "CREATE INDEX `idx_processors_SIMD_registers` ON `processors` (`SIMD_registers`);\n"     +
                "CREATE INDEX `idx_processors_SIMD_size` ON `processors` (`SIMD_size`);\n"               +
                "CREATE INDEX `idx_processors_L1_instruction` ON `processors` (`L1_instruction`);\n"     +
                "CREATE INDEX `idx_processors_L1_data` ON `processors` (`L1_data`);\n"                   +
                "CREATE INDEX `idx_processors_L2` ON `processors` (`L2`);\n"                             +
                "CREATE INDEX `idx_processors_L3` ON `processors` (`L3`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new foreign keys to table `processors`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `processors` ADD FOREIGN KEY `fk_processors_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;\n" +
                "ALTER TABLE `processors` ADD FOREIGN KEY `fk_processors_instruction_set` (instruction_set) REFERENCES `instruction_sets` (`id`) ON UPDATE CASCADE;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 10...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('10')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV11()
        {
            Console.WriteLine("Updating database to version 11");

            Console.WriteLine("Adding new columns to table `gpus`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `gpus` ADD COLUMN `company` INT NULL;\n"            +
                                "ALTER TABLE `gpus` ADD COLUMN `model_code` VARCHAR(45) NULL;\n" +
                                "ALTER TABLE `gpus` ADD COLUMN `introduced` DATETIME NULL;\n"    +
                                "ALTER TABLE `gpus` ADD COLUMN `package` VARCHAR(45) NULL;\n"    +
                                "ALTER TABLE `gpus` ADD COLUMN `process` VARCHAR(45) NULL;\n"    +
                                "ALTER TABLE `gpus` ADD COLUMN `process_nm` FLOAT NULL;\n"       +
                                "ALTER TABLE `gpus` ADD COLUMN `die_size` FLOAT NULL;\n"         +
                                "ALTER TABLE `gpus` ADD COLUMN `transistors` BIGINT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new indexes to table `gpus`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX `idx_gpus_company` ON `gpus` (`company`);\n"       +
                                "CREATE INDEX `idx_gpus_model_code` ON `gpus` (`model_code`);\n" +
                                "CREATE INDEX `idx_gpus_introduced` ON `gpus` (`introduced`);\n" +
                                "CREATE INDEX `idx_gpus_package` ON `gpus` (`package`);\n"       +
                                "CREATE INDEX `idx_gpus_process` ON `gpus` (`process`);\n"       +
                                "CREATE INDEX `idx_gpus_process_nm` ON `gpus` (`process_nm`);\n" +
                                "CREATE INDEX `idx_gpus_die_size` ON `gpus` (`die_size`);\n"     +
                                "CREATE INDEX `idx_gpus_transistors` ON `gpus` (`transistors`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new foreign keys to table `gpus`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `gpus` ADD FOREIGN KEY `fk_gpus_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 11...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('11')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
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