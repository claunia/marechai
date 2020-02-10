/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V11.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 11.
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

namespace Marechai.Database.Schemas.Sql
{
    public static class V11
    {
        public static readonly string Admins = V10.Admins;

        public static readonly string BrowserTests = V10.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                               "INSERT INTO marechai_db (version) VALUES ('11');";

        public static readonly string Companies = V10.Companies;

        public static readonly string Computers = V10.Computers;

        public static readonly string Consoles = V10.Consoles;

        public static readonly string DiskFormats = V10.DiskFormats;

        public static readonly string Forbidden = V10.Forbidden;

        public static readonly string Gpus = "CREATE TABLE `gpus` (\n"                       +
                                             "`id` int(11)     NOT NULL AUTO_INCREMENT,\n"   +
                                             "`name` char(128) NOT NULL DEFAULT '',\n"       +
                                             "`company` int(11)    DEFAULT NULL,\n"          +
                                             "`model_code` varchar(45) DEFAULT NULL,\n"      +
                                             "`introduced` datetime DEFAULT NULL,\n"         +
                                             "`package` varchar(45) DEFAULT NULL,\n"         +
                                             "`process` varchar(45) DEFAULT NULL,\n"         +
                                             "`process_nm` float DEFAULT NULL,\n"            +
                                             "`die_size` float       DEFAULT NULL,\n"        +
                                             "`transistors` bigint(20) DEFAULT NULL,\n"      + "PRIMARY KEY (`id`),\n" +
                                             "KEY `idx_gpus_name` (`name`),\n"               +
                                             "KEY `idx_gpus_company` (`company`),\n"         +
                                             "KEY `idx_gpus_model_code` (`model_code`),\n"   +
                                             "KEY `idx_gpus_introduced` (`introduced`),\n"   +
                                             "KEY `idx_gpus_package` (`package`),\n"         +
                                             "KEY `idx_gpus_process` (`process`),\n"         +
                                             "KEY `idx_gpus_process_nm` (`process_nm`),\n"   +
                                             "KEY `idx_gpus_die_size` (`die_size`),\n"       +
                                             "KEY `idx_gpus_transistors` (`transistors`),\n" +
                                             "CONSTRAINT `fk_gpus_company` FOREIGN KEY (`company`) REFERENCES `companies` (`id`) ON UPDATE CASCADE);";

        public static readonly string Logs = V10.Logs;

        public static readonly string MoneyDonations = V10.MoneyDonations;

        public static readonly string MusicSynths = V10.MusicSynths;

        public static readonly string News = V10.News;

        public static readonly string OwnedComputers = V10.OwnedComputers;

        public static readonly string OwnedConsoles = V10.OwnedConsoles;

        public static readonly string Processors = V10.Processors;

        public static readonly string SoundSynths = V10.SoundSynths;

        public static readonly string ComputersForeignKeys = V10.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V10.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = V10.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V10.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V10.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V10.CompanyLogos;

        public static readonly string CompanyDescriptions = V10.CompanyDescriptions;

        public static readonly string InstructionSets = V10.InstructionSets;

        public static readonly string InstructionSetExtensions = V10.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V10.InstructionSetExtensionsByProcessor;
    }
}