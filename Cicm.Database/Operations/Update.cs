/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
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
using Cicm.Database.Schemas;
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
                    case 11:
                    {
                        UpdateDatabaseToV12();
                        break;
                    }
                    case 12:
                    {
                        UpdateDatabaseToV13();
                        break;
                    }
                    case 13:
                    {
                        UpdateDatabaseToV14();
                        break;
                    }
                    case 14:
                    {
                        UpdateDatabaseToV15();
                        break;
                    }
                    case 15:
                    {
                        UpdateDatabaseToV16();
                        break;
                    }
                    case 16:
                    {
                        UpdateDatabaseToV17();
                        break;
                    }
                    case 17:
                    {
                        UpdateDatabaseToV18();
                        break;
                    }
                    case 18:
                    {
                        UpdateDatabaseToV19();
                        break;
                    }
                    case 19:
                    {
                        UpdateDatabaseToV20();
                        break;
                    }
                    case 20:
                    {
                        UpdateDatabaseToV21();
                        break;
                    }
                    case 21:
                    {
                        UpdateDatabaseToV22();
                        break;
                    }
                    case 22:
                    {
                        UpdateDatabaseToV23();
                        break;
                    }
                    case 23:
                    {
                        UpdateVersionToEntityFramework();
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
                "ALTER TABLE `gpus` ADD FOREIGN KEY `fk_gpus_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 11...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('11')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV12()
        {
            Console.WriteLine("Updating database to version 12");

            Console.WriteLine("Altering colums from table `computers`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `computers` CHANGE COLUMN `gpu` `gpu` INT DEFAULT NULL;\n"   +
                                "ALTER TABLE `computers` CHANGE COLUMN `cpu1` `cpu1` INT DEFAULT NULL;\n" +
                                "ALTER TABLE `computers` CHANGE COLUMN `mhz1` `mhz1` INT DEFAULT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Altering colums from table `consoles`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `consoles` CHANGE COLUMN `gpu` `gpu` INT DEFAULT NULL;\n"   +
                                "ALTER TABLE `consoles` CHANGE COLUMN `cpu1` `cpu1` INT DEFAULT NULL;\n" +
                                "ALTER TABLE `consoles` CHANGE COLUMN `mhz1` `mhz1` INT DEFAULT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new special items to table `gpus`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = $"INSERT INTO `gpus` (`id`, `name`) VALUES ({DB_NONE}, 'DB_NONE');\n" +
                                $"INSERT INTO `gpus` (`id`, `name`) VALUES ({DB_SOFTWARE}, 'DB_FRAMEBUFFER');";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Updating items from table `computers`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = $"UPDATE `computers` SET `gpu` = {DB_NONE} WHERE `gpu` = 1;\n" +
                                "UPDATE `computers` SET `gpu` = NULL WHERE `gpu` = 2;\n"       +
                                $"UPDATE `computers` SET `gpu` = {DB_SOFTWARE} WHERE `gpu` = 3;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Updating items from table `consoles`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = $"UPDATE `consoles` SET `gpu` = {DB_NONE} WHERE `gpu` = 1;\n" +
                                "UPDATE `consoles` SET `gpu` = NULL WHERE `gpu` = 2;\n"       +
                                $"UPDATE `consoles` SET `gpu` = {DB_SOFTWARE} WHERE `gpu` = 3;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Remocing old special items from table `gpus`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "DELETE FROM `gpus` WHERE `id` = 1;\n" + "DELETE FROM `gpus` WHERE `id` = 2;\n" +
                                "DELETE FROM `gpus` WHERE `id` = 3;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 12...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('12')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV13()
        {
            Console.WriteLine("Updating database to version 13");

            Console.WriteLine("Adding new columns to table `sound_synths`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `sound_synths` ADD COLUMN `company` INT NULL;\n"            +
                                "ALTER TABLE `sound_synths` ADD COLUMN `model_code` VARCHAR(45) NULL;\n" +
                                "ALTER TABLE `sound_synths` ADD COLUMN `introduced` DATETIME NULL;\n"    +
                                "ALTER TABLE `sound_synths` ADD COLUMN `voices` INT  NULL;\n"            +
                                "ALTER TABLE `sound_synths` ADD COLUMN `frequency` DOUBLE NULL;\n"       +
                                "ALTER TABLE `sound_synths` ADD COLUMN `depth` INT NULL;\n"              +
                                "ALTER TABLE `sound_synths` ADD COLUMN `square_wave` INT NULL;\n"        +
                                "ALTER TABLE `sound_synths` ADD COLUMN `white_noise` INT NULL;\n"        +
                                "ALTER TABLE `sound_synths` ADD COLUMN `type` INT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating new indexes in table `sound_synths`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX `idx_sound_synths_company` ON `sound_synths` (`company`);\n"         +
                                "CREATE INDEX `idx_sound_synths_model_code` ON `sound_synths` (`model_code`);\n"   +
                                "CREATE INDEX `idx_sound_synths_introduced` ON `sound_synths` (`introduced`);\n"   +
                                "CREATE INDEX `idx_sound_synths_voices` ON `sound_synths` (`voices`);\n"           +
                                "CREATE INDEX `idx_sound_synths_frequency` ON `sound_synths` (`frequency`);\n"     +
                                "CREATE INDEX `idx_sound_synths_depth` ON `sound_synths` (`depth`);\n"             +
                                "CREATE INDEX `idx_sound_synths_square_wave` ON `sound_synths` (`square_wave`);\n" +
                                "CREATE INDEX `idx_sound_synths_white_noise` ON `sound_synths` (`white_noise`);\n" +
                                "CREATE INDEX `idx_sound_synths_type` ON `sound_synths` (`type`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating foreign keys in table `sound_synths`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `sound_synths` ADD FOREIGN KEY `fk_sound_synths_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Dropping foreign keys from tables `computers` and `consoles`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `computers` DROP FOREIGN KEY `fk_computers_music_synth`;\n" +
                                "ALTER TABLE `consoles` DROP FOREIGN KEY `fk_consoles_music_synth`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `music_synths`");

            Dictionary<int, string> musicSynths = new Dictionary<int, string>();
            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from music_synths";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                musicSynths.Add(int.Parse(dataRow["id"].ToString()), dataRow["name"].ToString());

            Dictionary<int, int> conversionEquivalents = new Dictionary<int, int>();

            Console.WriteLine("Converting all items from `music_synths` to `sound_synths`");
            foreach(KeyValuePair<int, string> musicSynth in musicSynths)
            {
                dbCmd                     = dbCon.CreateCommand();
                dataAdapter               = dbCore.GetNewDataAdapter();
                dbCmd.CommandText         = $"SELECT * from sound_synths WHERE name LIKE '{musicSynth.Value}'";
                dataSet                   = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                if(dataSet.Tables[0].Rows.Count == 1)
                {
                    Console.WriteLine("Converting music synth `{0}` to sound synth `{1}`", musicSynth.Value,
                                      dataSet.Tables[0].Rows[0]["name"]);
                    conversionEquivalents.Add(musicSynth.Key, int.Parse(dataSet.Tables[0].Rows[0]["id"].ToString()));
                }
                else
                {
                    Console.Write("Adding new sound synth `{0}`... ", musicSynth.Value);
                    dbCmd             = dbCon.CreateCommand();
                    trans             = dbCon.BeginTransaction();
                    dbCmd.Transaction = trans;
                    dbCmd.CommandText = $"INSERT INTO sound_synths (name) VALUES ('{musicSynth.Value}')";

                    dbCmd.ExecuteNonQuery();
                    trans.Commit();
                    dbCmd.Dispose();

                    long id = dbCore.LastInsertRowId;
                    Console.WriteLine("got id {0}", id);
                    conversionEquivalents.Add(musicSynth.Key, (int)id);
                }
            }

            Console.WriteLine("Getting all items from `consoles`");
            Dictionary<int, int> consoleIdAndMusicSynthId = new Dictionary<int, int>();
            dbCmd                     = dbCon.CreateCommand();
            dataAdapter               = dbCore.GetNewDataAdapter();
            dbCmd.CommandText         = "SELECT id,music_synth from consoles";
            dataSet                   = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);
            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                consoleIdAndMusicSynthId.Add(int.Parse(dataRow["id"].ToString()),
                                             int.Parse(dataRow["music_synth"].ToString()));

            trans = dbCon.BeginTransaction();
            foreach(KeyValuePair<int, int> keyValuePair in consoleIdAndMusicSynthId)
            {
                conversionEquivalents.TryGetValue(keyValuePair.Value, out int newId);
                Console.WriteLine("Converting music synth {0} to sound synth {1} for console {2}... ",
                                  keyValuePair.Value, newId, keyValuePair.Key);
                dbCmd             = dbCon.CreateCommand();
                dbCmd.Transaction = trans;
                dbCmd.CommandText = $"UPDATE consoles SET music_synth = {newId} WHERE id = {keyValuePair.Key}";
                dbCmd.ExecuteNonQuery();
                dbCmd.Dispose();
            }

            Console.WriteLine("Comitting changes...");
            trans.Commit();

            Console.WriteLine("Getting all items from `computers`");
            Dictionary<int, int> computerIdAndMusicSynthId = new Dictionary<int, int>();
            dbCmd                     = dbCon.CreateCommand();
            dataAdapter               = dbCore.GetNewDataAdapter();
            dbCmd.CommandText         = "SELECT id,music_synth from computers";
            dataSet                   = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);
            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                computerIdAndMusicSynthId.Add(int.Parse(dataRow["id"].ToString()),
                                              int.Parse(dataRow["music_synth"].ToString()));

            trans = dbCon.BeginTransaction();
            foreach(KeyValuePair<int, int> keyValuePair in computerIdAndMusicSynthId)
            {
                conversionEquivalents.TryGetValue(keyValuePair.Value, out int newId);
                Console.WriteLine("Converting music synth {0} to sound synth {1} for computer {2}... ",
                                  keyValuePair.Value, newId, keyValuePair.Key);
                dbCmd             = dbCon.CreateCommand();
                dbCmd.Transaction = trans;
                dbCmd.CommandText = $"UPDATE computers SET music_synth = {newId} WHERE id = {keyValuePair.Key}";
                dbCmd.ExecuteNonQuery();
                dbCmd.Dispose();
            }

            Console.WriteLine("Comitting changes...");
            trans.Commit();

            Console.WriteLine("Adding new foreign keys to table `computers`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `computers` ADD FOREIGN KEY `fk_computers_music_synth` (music_synth) REFERENCES `sound_synths` (`id`) ON UPDATE CASCADE;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new foreign keys to table `consoles`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `consoles` ADD FOREIGN KEY `fk_consoles_music_synth` (music_synth) REFERENCES `sound_synths` (`id`) ON UPDATE CASCADE;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Dropping table `music_synths`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "DROP TABLE `music_synths`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 13...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('13')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV14()
        {
            Console.WriteLine("Updating database to version 14");

            Console.WriteLine("Renaming table `computers` to `machines`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `computers` RENAME TO `machines`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Removing column `bits` from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP COLUMN `bits`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating column `type` in table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                $"ALTER TABLE `machines` ADD COLUMN `type` INT NOT NULL DEFAULT '{(int)MachineType.Unknown}';";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Updating all entries in table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = $"UPDATE `machines` SET `type` = '{(int)MachineType.Computer}';";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Renaming all indexes on table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `machines` DROP INDEX `idx_computers_company`, ADD INDEX `idx_machines_company` (`company`);\n"             +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_year`, ADD INDEX `idx_machines_year` (`year`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_model`, ADD INDEX `idx_machines_model` (`model`);\n"                   +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_cpu1`, ADD INDEX `idx_machines_cpu1` (`cpu1`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_cpu2`, ADD INDEX `idx_machines_cpu2` (`cpu2`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_mhz1`, ADD INDEX `idx_machines_mhz1` (`mhz1`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_mhz2`, ADD INDEX `idx_machines_mhz2` (`mhz2`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_ram`, ADD INDEX `idx_machines_ram` (`ram`);\n"                         +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_rom`, ADD INDEX `idx_machines_rom` (`rom`);\n"                         +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_gpu`, ADD INDEX `idx_machines_gpu` (`gpu`);\n"                         +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_vram`, ADD INDEX `idx_machines_vram` (`vram`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_colors`, ADD INDEX `idx_machines_colors` (`colors`);\n"                +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_res`, ADD INDEX `idx_machines_res` (`res`);\n"                         +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_sound_synth`, ADD INDEX `idx_machines_sound_synth` (`sound_synth`);\n" +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_music_synth`, ADD INDEX `idx_machines_music_synth` (`music_synth`);\n" +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_hdd1`, ADD INDEX `idx_machines_hdd1` (`hdd1`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_hdd2`, ADD INDEX `idx_machines_hdd2` (`hdd2`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_hdd3`, ADD INDEX `idx_machines_hdd3` (`hdd3`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_disk1`, ADD INDEX `idx_machines_disk1` (`disk1`);\n"                   +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_disk2`, ADD INDEX `idx_machines_disk2` (`disk2`);\n"                   +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_cap1`, ADD INDEX `idx_machines_cap1` (`cap1`);\n"                      +
                "ALTER TABLE `machines` DROP INDEX `idx_computers_cap2`, ADD INDEX `idx_machines_cap2` (`cap2`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Removing old foreign keys from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_company`;\n"     +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_cpu1`;\n"        +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_cpu2`;\n"        +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_disk1`;\n"       +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_disk2`;\n"       +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_gpu`;\n"         +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_hdd1`;\n"        +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_hdd2`;\n"        +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_hdd3`;\n"        +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_music_synth`;\n" +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_computers_sound_synth`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding foreign keys in table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;\n"            +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_cpu1` (cpu1) REFERENCES `processors` (`id`) ON UPDATE CASCADE;\n"                 +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_cpu2` (cpu2) REFERENCES `processors` (`id`) ON UPDATE CASCADE;\n"                 +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_disk1` (disk1) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"             +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_disk2` (disk2) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"             +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_gpu` (gpu) REFERENCES `gpus` (`id`) ON UPDATE CASCADE;\n"                         +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd1` (hdd1) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"               +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd2` (hdd2) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"               +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd3` (hdd3) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"               +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_music_synth` (music_synth) REFERENCES `sound_synths` (`id`) ON UPDATE CASCADE;\n" +
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_sound_synth` (sound_synth) REFERENCES `sound_synths` (`id`) ON UPDATE CASCADE;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new index for `type` in table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX `idx_machines_type` ON `machines` (`type`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `consoles`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from consoles";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1  = dbcmd.CreateParameter();
                IDbDataParameter param2  = dbcmd.CreateParameter();
                IDbDataParameter param3  = dbcmd.CreateParameter();
                IDbDataParameter param4  = dbcmd.CreateParameter();
                IDbDataParameter param5  = dbcmd.CreateParameter();
                IDbDataParameter param6  = dbcmd.CreateParameter();
                IDbDataParameter param7  = dbcmd.CreateParameter();
                IDbDataParameter param8  = dbcmd.CreateParameter();
                IDbDataParameter param9  = dbcmd.CreateParameter();
                IDbDataParameter param10 = dbcmd.CreateParameter();
                IDbDataParameter param11 = dbcmd.CreateParameter();
                IDbDataParameter param12 = dbcmd.CreateParameter();
                IDbDataParameter param13 = dbcmd.CreateParameter();
                IDbDataParameter param14 = dbcmd.CreateParameter();
                IDbDataParameter param15 = dbcmd.CreateParameter();
                IDbDataParameter param16 = dbcmd.CreateParameter();
                IDbDataParameter param17 = dbcmd.CreateParameter();
                IDbDataParameter param18 = dbcmd.CreateParameter();
                IDbDataParameter param19 = dbcmd.CreateParameter();
                IDbDataParameter param20 = dbcmd.CreateParameter();
                IDbDataParameter param21 = dbcmd.CreateParameter();

                param1.ParameterName  = "@company";
                param2.ParameterName  = "@year";
                param3.ParameterName  = "@model";
                param4.ParameterName  = "@cpu1";
                param5.ParameterName  = "@mhz1";
                param6.ParameterName  = "@cpu2";
                param7.ParameterName  = "@mhz2";
                param8.ParameterName  = "@ram";
                param9.ParameterName  = "@rom";
                param10.ParameterName = "@gpu";
                param11.ParameterName = "@vram";
                param12.ParameterName = "@colors";
                param13.ParameterName = "@res";
                param14.ParameterName = "@sound_synth";
                param15.ParameterName = "@music_synth";
                param16.ParameterName = "@sound_channels";
                param17.ParameterName = "@music_channels";
                param18.ParameterName = "@type";
                param19.ParameterName = "@hdd1";
                param20.ParameterName = "@disk1";
                param21.ParameterName = "@cap1";

                param1.DbType  = DbType.Int32;
                param2.DbType  = DbType.Int32;
                param3.DbType  = DbType.String;
                param4.DbType  = DbType.Int32;
                param5.DbType  = DbType.Double;
                param6.DbType  = DbType.Int32;
                param7.DbType  = DbType.Double;
                param8.DbType  = DbType.Int32;
                param9.DbType  = DbType.Int32;
                param10.DbType = DbType.Int32;
                param11.DbType = DbType.Int32;
                param12.DbType = DbType.Int32;
                param13.DbType = DbType.String;
                param14.DbType = DbType.Int32;
                param15.DbType = DbType.Int32;
                param16.DbType = DbType.Int32;
                param17.DbType = DbType.Int32;
                param18.DbType = DbType.Int32;
                param19.DbType = DbType.Int32;
                param20.DbType = DbType.Int32;
                param21.DbType = DbType.Int32;

                param1.Value = (int)dataRow["company"];
                param2.Value = (int)dataRow["year"];
                param3.Value = (string)dataRow["model"];
                param4.Value = dataRow["cpu1"] == DBNull.Value ? (object)null : (int)dataRow["cpu1"];
                param5.Value = dataRow["mhz1"] == DBNull.Value
                                    ? (object)null
                                    : float.Parse(dataRow["mhz1"].ToString());
                param6.Value = dataRow["cpu2"] == DBNull.Value ? (object)null : (int)dataRow["cpu2"];
                param7.Value = dataRow["mhz2"] == DBNull.Value
                                    ? (object)null
                                    : float.Parse(dataRow["mhz2"].ToString());
                param8.Value  = (int)dataRow["ram"];
                param9.Value  = (int)dataRow["rom"];
                param10.Value = dataRow["gpu"] == DBNull.Value ? (object)null : (int)dataRow["gpu"];
                param11.Value = (int)dataRow["vram"];
                param12.Value = (int)dataRow["colors"];
                param13.Value = (string)dataRow["res"];
                param14.Value = (int)dataRow["sound_synth"];
                param15.Value = (int)dataRow["music_synth"];
                param16.Value = (int)dataRow["schannels"];
                param17.Value = (int)dataRow["mchannels"];
                param18.Value = MachineType.Console;
                param19.Value = 30;
                param20.Value = 30;
                param21.Value = 0;

                dbcmd.Parameters.Add(param1);
                dbcmd.Parameters.Add(param2);
                dbcmd.Parameters.Add(param3);
                dbcmd.Parameters.Add(param4);
                dbcmd.Parameters.Add(param5);
                dbcmd.Parameters.Add(param6);
                dbcmd.Parameters.Add(param7);
                dbcmd.Parameters.Add(param8);
                dbcmd.Parameters.Add(param9);
                dbcmd.Parameters.Add(param10);
                dbcmd.Parameters.Add(param11);
                dbcmd.Parameters.Add(param12);
                dbcmd.Parameters.Add(param13);
                dbcmd.Parameters.Add(param14);
                dbcmd.Parameters.Add(param15);
                dbcmd.Parameters.Add(param16);
                dbcmd.Parameters.Add(param17);
                dbcmd.Parameters.Add(param18);
                dbcmd.Parameters.Add(param19);
                dbcmd.Parameters.Add(param20);
                dbcmd.Parameters.Add(param21);

                trans             = dbCon.BeginTransaction();
                dbcmd.Transaction = trans;

                Console.WriteLine("Converting console \"{0}\" to machine", (string)param3.Value);

                const string SQL =
                    "INSERT INTO machines (company, year, model, cpu1, mhz1, cpu2, mhz2, ram, rom, gpu, vram, colors, res, sound_synth, music_synth, sound_channels, music_channels, type, hdd1, disk1, cap1)" +
                    " VALUES (@company, @year, @model, @cpu1, @mhz1, @cpu2, @mhz2, @ram, @rom, @gpu, @vram, @colors, @res, @sound_synth, @music_synth, @sound_channels, @music_channels, @type, @hdd1, @disk1, @cap1)";

                dbcmd.CommandText = SQL;

                dbcmd.ExecuteNonQuery();
                trans.Commit();
                dbcmd.Dispose();
            }

            Console.WriteLine("Dropping table `consoles`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "DROP TABLE `consoles`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 14...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('14')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV15()
        {
            Console.WriteLine("Updating database to version 15");

            Console.WriteLine("Creating table `processors_by_machine`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V15.ProcessorsByMachine;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `machines`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from machines";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1 = dbcmd.CreateParameter();
                IDbDataParameter param2 = dbcmd.CreateParameter();
                IDbDataParameter param3 = dbcmd.CreateParameter();

                param1.ParameterName = "@machine";
                param2.ParameterName = "@processor";
                param3.ParameterName = "@speed";

                param1.DbType = DbType.Int32;
                param2.DbType = DbType.Int32;
                param3.DbType = DbType.Double;

                param1.Value = (int)dataRow["id"];

                const string SQL =
                    "INSERT INTO processors_by_machine (machine, processor, speed) VALUES (@machine, @processor, @speed)";

                dbcmd.Parameters.Add(param1);
                dbcmd.Parameters.Add(param2);
                dbcmd.Parameters.Add(param3);

                if(dataRow["cpu1"] != DBNull.Value)
                {
                    param2.Value = (int)dataRow["cpu1"];
                    param3.Value = dataRow["mhz1"] == DBNull.Value
                                       ? (object)null
                                       : float.Parse(dataRow["mhz1"].ToString());

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding processor {0} to machine {1}", (int)param2.Value, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                if(dataRow["cpu2"] != DBNull.Value)
                {
                    param2.Value = (int)dataRow["cpu2"];
                    param3.Value = dataRow["mhz2"] == DBNull.Value
                                       ? (object)null
                                       : float.Parse(dataRow["mhz2"].ToString());

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding processor {0} to machine {1}", (int)param2.Value, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                dbcmd.Dispose();
            }

            Console.WriteLine("Removing processor columns from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_cpu1`;\n" +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_cpu2`;\n" +
                                "ALTER TABLE `machines` DROP COLUMN `cpu1`;\n"                  +
                                "ALTER TABLE `machines` DROP COLUMN `cpu2`;\n"                  +
                                "ALTER TABLE `machines` DROP COLUMN `mhz1`;\n"                  +
                                "ALTER TABLE `machines` DROP COLUMN `mhz2`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 15...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('15')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV16()
        {
            Console.WriteLine("Updating database to version 16");

            Console.WriteLine("Creating table `gpus_by_machine`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V16.GpusByMachine;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `machines`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from machines";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1 = dbcmd.CreateParameter();
                IDbDataParameter param2 = dbcmd.CreateParameter();

                param1.ParameterName = "@machine";
                param2.ParameterName = "@gpu";

                param1.DbType = DbType.Int32;
                param2.DbType = DbType.Int32;

                param1.Value = (int)dataRow["id"];

                const string SQL = "INSERT INTO gpus_by_machine (machine, gpu) VALUES (@machine, @gpu)";

                dbcmd.Parameters.Add(param1);
                dbcmd.Parameters.Add(param2);

                if(dataRow["gpu"] != DBNull.Value)
                {
                    param2.Value = (int)dataRow["gpu"];

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding gpu {0} to machine {1}", (int)param2.Value, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                dbcmd.Dispose();
            }

            Console.WriteLine("Removing processor columns from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_gpu`;\n" +
                                "ALTER TABLE `machines` DROP COLUMN `gpu`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 16...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('16')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV17()
        {
            Console.WriteLine("Updating database to version 17");

            Console.WriteLine("Creating table `sound_by_machine`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V17.SoundByMachine;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new special items to table `sound_synths`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = $"INSERT INTO `sound_synths` (`id`, `name`) VALUES ({DB_SOFTWARE}, 'DB_SOFTWARE');";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Changing sound and music synths in machine from 27 to {0}", DB_SOFTWARE);
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = $"UPDATE `machines` SET sound_synth = {DB_SOFTWARE} WHERE sound_synth = 27;\n" +
                                $"UPDATE `machines` SET music_synth = {DB_SOFTWARE} WHERE music_synth = 27;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `machines`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from machines";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1 = dbcmd.CreateParameter();
                IDbDataParameter param2 = dbcmd.CreateParameter();

                param1.ParameterName = "@machine";
                param2.ParameterName = "@sound_synth";

                param1.DbType = DbType.Int32;
                param2.DbType = DbType.Int32;

                param1.Value = (int)dataRow["id"];

                const string SQL =
                    "INSERT INTO sound_by_machine (machine, sound_synth) VALUES (@machine, @sound_synth)";

                dbcmd.Parameters.Add(param1);
                dbcmd.Parameters.Add(param2);

                if(dataRow["sound_synth"]      != DBNull.Value && (int)dataRow["sound_synth"] != 1 &&
                   (int)dataRow["sound_synth"] != 2)
                {
                    param2.Value = (int)dataRow["sound_synth"];

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding sound {0} to machine {1}", (int)param2.Value, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                if(dataRow["music_synth"]      != DBNull.Value && (int)dataRow["music_synth"] != 1 &&
                   (int)dataRow["music_synth"] != 2)
                {
                    param2.Value = (int)dataRow["music_synth"];

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding sound {0} to machine {1}", (int)param2.Value, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                dbcmd.Dispose();
            }

            Console.WriteLine("Removing sound columns from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_sound_synth`;\n" +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_music_synth`;\n" +
                                "ALTER TABLE `machines` DROP COLUMN `sound_channels`;\n"               +
                                "ALTER TABLE `machines` DROP COLUMN `music_channels`;\n"               +
                                "ALTER TABLE `machines` DROP COLUMN `sound_synth`;\n"                  +
                                "ALTER TABLE `machines` DROP COLUMN `music_synth`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Removing old sound items `sound_synths`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "DELETE FROM sound_synths WHERE id = 1;\n" +
                                "DELETE FROM sound_synths WHERE id = 2;\n" + "DELETE FROM sound_synths WHERE id = 27;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 17...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('17')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV18()
        {
            Console.WriteLine("Updating database to version 18");

            Console.WriteLine("Creating table `memory_by_machine`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V18.MemoryByMachine;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `machines`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from machines";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1 = dbcmd.CreateParameter();
                IDbDataParameter param2 = dbcmd.CreateParameter();
                IDbDataParameter param3 = dbcmd.CreateParameter();

                param1.ParameterName = "@machine";
                param2.ParameterName = "@usage";
                param3.ParameterName = "@size";

                param1.DbType = DbType.Int32;
                param2.DbType = DbType.Int32;
                param3.DbType = DbType.Int64;

                param1.Value = (int)dataRow["id"];

                const string SQL =
                    "INSERT INTO `memory_by_machine` (`machine`, `usage`, `size`) VALUES (@machine, @usage, @size)";

                dbcmd.Parameters.Add(param1);
                dbcmd.Parameters.Add(param2);
                dbcmd.Parameters.Add(param3);

                if(dataRow["ram"] != DBNull.Value && (int)dataRow["ram"] > 0)
                {
                    param2.Value = MemoryUsage.Work;
                    param3.Value = (int)dataRow["ram"] * 1024;

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding {0} bytes of {1} memory to machine {2}", (int)param3.Value,
                                      MemoryUsage.Work, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                if(dataRow["rom"] != DBNull.Value && (int)dataRow["rom"] > 0)
                {
                    param2.Value = MemoryUsage.Firmware;
                    param3.Value = (int)dataRow["rom"] * 1024;

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding {0} bytes of {1} memory to machine {2}", (int)param3.Value,
                                      MemoryUsage.Firmware, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                if(dataRow["vram"] != DBNull.Value && (int)dataRow["vram"] > 0)
                {
                    param2.Value = MemoryUsage.Video;
                    param3.Value = (int)dataRow["vram"] * 1024;

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding {0} bytes of {1} memory to machine {2}", (int)param3.Value,
                                      MemoryUsage.Video, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                dbcmd.Dispose();
            }

            Console.WriteLine("Removing memory columns from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP COLUMN `ram`;\n" +
                                "ALTER TABLE `machines` DROP COLUMN `rom`;\n" +
                                "ALTER TABLE `machines` DROP COLUMN `vram`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 18...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('18')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV19()
        {
            Console.WriteLine("Updating database to version 19");

            Console.WriteLine("Creating table `resolutions`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V19.Resolutions;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Creating table `resolutions_by_gpu`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V19.ResolutionsByGpu;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `machines`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from machines";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                if(dataRow["colors"]      == DBNull.Value || dataRow["res"] == DBNull.Value ||
                   (int)dataRow["colors"] == 0            ||
                   (string)dataRow["res"] == "???") continue;

                dbCmd = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter2 = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = $"SELECT * FROM gpus_by_machine WHERE machine = {(int)dataRow["id"]}";
                DataSet dataSet2 = new DataSet();
                dataAdapter2.SelectCommand = dbCmd;
                dataAdapter2.Fill(dataSet2);

                if(dataSet2.Tables[0].Rows.Count == 0) continue;

                int gpuId = (int)dataSet2.Tables[0].Rows[0]["gpu"];

                string[] resPieces = ((string)dataRow["res"]).Split('x');

                if(!int.TryParse(resPieces[0], out int width)) continue;
                if(!int.TryParse(resPieces[1], out int height)) continue;

                dbCmd        = dbCon.CreateCommand();
                dataAdapter2 = dbCore.GetNewDataAdapter();
                dbCmd.CommandText =
                    $"SELECT * FROM resolutions WHERE width = {width} AND height = {height} AND colors = {(int)dataRow["colors"]}";
                dataSet2                   = new DataSet();
                dataAdapter2.SelectCommand = dbCmd;
                dataAdapter2.Fill(dataSet2);

                int    resId;
                string sql;

                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1 = dbcmd.CreateParameter();
                IDbDataParameter param2 = dbcmd.CreateParameter();
                IDbDataParameter param3 = dbcmd.CreateParameter();

                if(dataSet2.Tables[0].Rows.Count == 0)
                {
                    param1.ParameterName = "@width";
                    param2.ParameterName = "@height";
                    param3.ParameterName = "@colors";

                    param1.DbType = DbType.Int32;
                    param2.DbType = DbType.Int32;
                    param3.DbType = DbType.Int64;

                    param1.Value = width;
                    param2.Value = height;
                    param3.Value = (int)dataRow["colors"];

                    sql = "INSERT INTO `resolutions` (`width`, `height`, `colors`) VALUES (@width, @height, @colors)";

                    dbcmd.Parameters.Add(param1);
                    dbcmd.Parameters.Add(param2);
                    dbcmd.Parameters.Add(param3);

                    dbcmd.CommandText = sql;

                    dbcmd.ExecuteNonQuery();
                    dbcmd.Dispose();

                    resId = (int)dbCore.LastInsertRowId;
                }
                else resId = (int)dataSet2.Tables[0].Rows[0]["id"];

                dbcmd = dbCon.CreateCommand();

                param1 = dbcmd.CreateParameter();
                param2 = dbcmd.CreateParameter();

                param1.ParameterName = "@gpu";
                param2.ParameterName = "@resolution";

                param1.DbType = DbType.Int32;
                param2.DbType = DbType.Int32;

                param1.Value = gpuId;
                param2.Value = resId;

                sql = "INSERT INTO `resolutions_by_gpu` (`gpu`, `resolution`) VALUES (@gpu, @resolution)";

                dbcmd.Parameters.Add(param1);
                dbcmd.Parameters.Add(param2);

                dbcmd.CommandText = sql;

                dbcmd.ExecuteNonQuery();
                dbcmd.Dispose();
            }

            Console.WriteLine("Removing resolution columns from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP COLUMN `res`;\n" +
                                "ALTER TABLE `machines` DROP COLUMN `colors`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 19...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('19')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV20()
        {
            Console.WriteLine("Updating database to version 20");

            Console.WriteLine("Creating table `storage_by_machine`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V20.StorageByMachine;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `machines`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from machines";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1 = dbcmd.CreateParameter();
                IDbDataParameter param2 = dbcmd.CreateParameter();
                IDbDataParameter param3 = dbcmd.CreateParameter();
                IDbDataParameter param4 = dbcmd.CreateParameter();

                param1.ParameterName = "@machine";
                param2.ParameterName = "@type";
                param3.ParameterName = "@interface";
                param4.ParameterName = "@capacity";

                param1.DbType = DbType.Int32;
                param2.DbType = DbType.Int32;
                param3.DbType = DbType.Int32;
                param4.DbType = DbType.Int64;

                param1.Value = (int)dataRow["id"];

                const string SQL =
                    "INSERT INTO `storage_by_machine` (`machine`, `type`, `interface`, `capacity`) VALUES (@machine, @type, @interface, @capacity)";

                dbcmd.Parameters.Add(param1);
                dbcmd.Parameters.Add(param2);
                dbcmd.Parameters.Add(param3);
                dbcmd.Parameters.Add(param4);

                foreach(string media in new[] {"hdd1", "hdd2", "hdd3", "disk1", "disk2"})
                {
                    if(dataRow[media] == DBNull.Value || (int)dataRow[media] == 30) continue;

                    param3.Value = StorageInterface.Unknown;

                    switch((int)dataRow[media])
                    {
                        case 1:
                            param2.Value = StorageType.CompactFloppy;
                            break;
                        case 3:
                        case 5:
                            param2.Value = StorageType.Microfloppy;
                            break;
                        case 4:
                            param2.Value = StorageType.Minifloppy;
                            break;
                        case 7:
                            param2.Value = StorageType.CompactDisc;
                            break;
                        case 8:
                            param2.Value = StorageType.CompactCassette;
                            break;
                        case 9:
                            param2.Value = StorageType.CompactFlash;
                            break;
                        case 11:
                            param2.Value = StorageType.Dvd;
                            break;
                        case 12:
                            param2.Value = StorageType.GDROM;
                            break;
                        case 13:
                            param2.Value = StorageType.ZIP100;
                            break;
                        case 14:
                            param2.Value = StorageType.LS120;
                            break;
                        case 15:
                            param2.Value = StorageType.MagnetoOptical;
                            break;
                        case 17:
                            param2.Value = StorageType.Microdrive;
                            break;
                        case 18:
                            param2.Value = StorageType.MMC;
                            break;
                        case 20:
                            param2.Value = StorageType.SecureDigital;
                            break;
                        case 21:
                            param2.Value = StorageType.SmartMedia;
                            break;
                        case 23:
                            param2.Value = StorageType.PunchedCard;
                            break;
                        case 24:
                            param2.Value = StorageType.HardDisk;
                            param3.Value = StorageInterface.ACSI;
                            break;
                        case 25:
                        case 29:
                            param2.Value = StorageType.HardDisk;
                            param3.Value = StorageInterface.ATA;
                            break;
                        case 26:
                            param2.Value = StorageType.HardDisk;
                            param3.Value = StorageInterface.ESDI;
                            break;
                        case 27:
                            param2.Value = StorageType.HardDisk;
                            param3.Value = StorageInterface.FireWire;
                            break;
                        case 28:
                            param2.Value = StorageType.CompactFloppy;
                            break;
                        case 32:
                        case 35:
                            param2.Value = StorageType.HardDisk;
                            param3.Value = StorageInterface.ST506;
                            break;
                        case 33:
                            param2.Value = StorageType.HardDisk;
                            param3.Value = StorageInterface.SASI;
                            break;
                        case 34:
                        case 41:
                            param2.Value = StorageType.HardDisk;
                            param3.Value = StorageInterface.SCSI;
                            break;
                        case 40:
                            param2.Value = StorageType.Floppy;
                            break;
                        case 43:
                            param2.Value = StorageType.Bluray;
                            break;
                        case 44:
                            param2.Value = StorageType.GOD;
                            break;
                        case 45:
                            param2.Value = StorageType.WOD;
                            break;
                        default:
                            param2.Value = StorageType.Unknown;
                            break;
                    }

                    param4.Value = null;

                    switch(media)
                    {
                        case "disk1":
                            if(dataRow["cap1"] != DBNull.Value)
                                if(int.TryParse((string)dataRow["cap1"], out int cap))
                                    param4.Value = cap == 0
                                                       ? (object)null
                                                       : (StorageType)param2.Value == StorageType.CompactCassette
                                                           ? cap
                                                           : cap * 1024;
                            break;
                        case "disk2":
                            if(dataRow["cap2"] != DBNull.Value)
                                if(int.TryParse((string)dataRow["cap2"], out int cap))
                                    param4.Value = cap == 0
                                                       ? (object)null
                                                       : (StorageType)param2.Value == StorageType.CompactCassette
                                                           ? cap
                                                           : cap * 1024;
                            break;
                    }

                    trans             = dbCon.BeginTransaction();
                    dbcmd.Transaction = trans;

                    Console.WriteLine("Adding storage type {0} with interface {1} to machine {2}",
                                      (StorageType)param2.Value, (StorageInterface)param3.Value, (int)param1.Value);

                    dbcmd.CommandText = SQL;

                    dbcmd.ExecuteNonQuery();
                    trans.Commit();
                }

                dbcmd.Dispose();
            }

            Console.WriteLine("Removing memory columns from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_disk1`;\n" +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_disk2`;\n" +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_hdd1`;\n"  +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_hdd2`;\n"  +
                                "ALTER TABLE `machines` DROP FOREIGN KEY `fk_machines_hdd3`;\n"  +
                                "ALTER TABLE `machines` DROP COLUMN `hdd1`;\n"                   +
                                "ALTER TABLE `machines` DROP COLUMN `hdd2`;\n"                   +
                                "ALTER TABLE `machines` DROP COLUMN `hdd3`;\n"                   +
                                "ALTER TABLE `machines` DROP COLUMN `disk1`;\n"                  +
                                "ALTER TABLE `machines` DROP COLUMN `cap1`;\n"                   +
                                "ALTER TABLE `machines` DROP COLUMN `disk2`;\n"                  +
                                "ALTER TABLE `machines` DROP COLUMN `cap2`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Removing table `disk_formats`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "DROP TABLE `disk_formats`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 20...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('20')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV21()
        {
            Console.WriteLine("Updating database to version 21");

            Console.WriteLine("Adding new columns to table `machines`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` ADD COLUMN `introduced` DATETIME NULL;\n" +
                                "CREATE INDEX idx_machines_introduced ON machines (introduced);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Getting all items from `machines`");

            dbCmd = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * from machines";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                IDbCommand dbcmd = dbCon.CreateCommand();

                IDbDataParameter param1 = dbcmd.CreateParameter();

                param1.ParameterName = "@introduced";

                param1.DbType = DbType.DateTime;

                if((int)dataRow["year"] > 0) param1.Value = new DateTime((int)dataRow["year"], 1, 1);
                else param1.Value                         = null;

                string sql = $"UPDATE `machines` SET introduced = @introduced WHERE id = {(int)dataRow["id"]}";

                dbcmd.Parameters.Add(param1);

                trans             = dbCon.BeginTransaction();
                dbcmd.Transaction = trans;

                Console.WriteLine("Adding introduction date {0} to machine {1}", (int)dataRow["year"],
                                  (int)dataRow["id"]);

                dbcmd.CommandText = sql;

                dbcmd.ExecuteNonQuery();
                trans.Commit();

                dbcmd.Dispose();
            }

            Console.WriteLine("Removing year column from table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` DROP COLUMN `year`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 21...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('21')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV22()
        {
            Console.WriteLine("Updating database to version 22");

            Console.WriteLine("Creating new table table `machine_families`");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = V22.MachineFamilies;
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new columns to table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE `machines` ADD COLUMN `family` INT DEFAULT NULL;\n"               +
                                "ALTER TABLE `machines` CHANGE COLUMN `model` `name` VARCHAR(255) NOT NULL;\n" +
                                "ALTER TABLE `machines` DROP INDEX `idx_machines_model`;\n"                    +
                                "ALTER TABLE `machines` ADD COLUMN `model` VARCHAR(50) DEFAULT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new indexes to table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "CREATE INDEX `idx_machines_family` ON `machines` (`family`);\n" +
                                "CREATE INDEX `idx_machines_name` ON `machines` (`name`);\n"     +
                                "CREATE INDEX `idx_machines_model` ON `machines` (`model`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding new foreign keys to table `machines`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_family` (family) REFERENCES machine_families (`id`) ON UPDATE CASCADE";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 22...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('22')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }

        void UpdateDatabaseToV23()
        {
            Console.WriteLine("Updating database to version 23");

            Console.WriteLine("Altering `browser_tests` primary key");
            IDbCommand     dbCmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE browser_tests MODIFY id int NOT NULL auto_increment;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Dropping foreign key between `iso3166_1_numeric` and `companies`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE companies DROP FOREIGN KEY `fk_companies_country`;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Altering `iso3166_1_numeric` primary key");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE iso3166_1_numeric MODIFY id smallint(3) NOT NULL;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Altering `country` column in `companies`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText = "ALTER TABLE companies MODIFY country smallint(3);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Re-adding new foreign keys to table `companies`");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE `companies` ADD FOREIGN KEY `fk_companies_country` (country) REFERENCES `iso3166_1_numeric` (`id`);";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Adding primary keys to several tables");
            dbCmd             = dbCon.CreateCommand();
            trans             = dbCon.BeginTransaction();
            dbCmd.Transaction = trans;
            dbCmd.CommandText =
                "ALTER TABLE gpus_by_machine       ADD id BIGINT NOT NULL PRIMARY KEY AUTO_INCREMENT;\n" +
                "ALTER TABLE memory_by_machine     ADD id BIGINT NOT NULL PRIMARY KEY AUTO_INCREMENT;\n" +
                "ALTER TABLE processors_by_machine ADD id BIGINT NOT NULL PRIMARY KEY AUTO_INCREMENT;\n" +
                "ALTER TABLE resolutions_by_gpu    ADD id BIGINT NOT NULL PRIMARY KEY AUTO_INCREMENT;\n" +
                "ALTER TABLE sound_by_machine      ADD id BIGINT NOT NULL PRIMARY KEY AUTO_INCREMENT;\n" +
                "ALTER TABLE storage_by_machine    ADD id BIGINT NOT NULL PRIMARY KEY AUTO_INCREMENT;";
            dbCmd.ExecuteNonQuery();
            trans.Commit();
            dbCmd.Dispose();

            Console.WriteLine("Setting new database version to 23...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('23')";
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

        public int GetVersion()
        {
            int            currentDbVersion = 2;
            IDbCommand     dbCmd            = dbCon.CreateCommand();
            IDbDataAdapter dataAdapter      = dbCore.GetNewDataAdapter();
            dbCmd.CommandText = "SELECT * FROM cicm_db";
            DataSet dataSet = new DataSet();
            dataAdapter.SelectCommand = dbCmd;
            dataAdapter.Fill(dataSet);

            foreach(DataRow dataRow in dataSet.Tables[0].Rows)
            {
                int newId                                     = int.Parse(dataRow["version"].ToString());
                if(newId > currentDbVersion) currentDbVersion = newId;
            }

            return currentDbVersion;
        }

        public void UpdateVersionToEntityFramework()
        {
            Console.WriteLine("Adding Entity Framework table...");
            IDbCommand dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText = "create table `__EFMigrationsHistory`\n"              +
                                "(MigrationId    varchar(95) not null primary key,\n" +
                                "ProductVersion varchar(32) not null);";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
            
            Console.WriteLine("Adding Entity Framework first migration...");
            dbCmd = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm.`__EFMigrationsHistory` (MigrationId, ProductVersion)" +
                                " VALUES ('20180805214952_InitialMigration', '2.1.1-rtm-30846');";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
            
            Console.WriteLine("Setting new database version to 1984 (Entity Framework)...");
            dbCmd             = dbCon.CreateCommand();
            dbCmd.CommandText = "INSERT INTO cicm_db (version) VALUES ('1984')";
            dbCmd.ExecuteNonQuery();
            dbCmd.Dispose();
        }
    }
}