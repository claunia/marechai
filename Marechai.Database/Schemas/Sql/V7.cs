/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V7.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 7.
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

namespace Marechai.Database.Schemas.Sql
{
    public static class V7
    {
        public static readonly string Admins = V6.Admins;

        public static readonly string BrowserTests = V6.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                               "INSERT INTO marechai_db (version) VALUES ('7');";

        public static readonly string Companies = "CREATE TABLE `companies` (\n"                            +
                                                  "`id` int(11)            NOT NULL AUTO_INCREMENT,\n"      +
                                                  "`name` varchar(128) NOT NULL DEFAULT '',\n"              +
                                                  "`founded` datetime          DEFAULT NULL,\n"             +
                                                  "`website` varchar(255) DEFAULT NULL,\n"                  +
                                                  "`twitter` varchar(45) DEFAULT NULL,\n"                   +
                                                  "`facebook` varchar(45) DEFAULT NULL,\n"                  +
                                                  "`sold` datetime DEFAULT NULL,\n"                         +
                                                  "`sold_to` int(11)   DEFAULT NULL,\n"                     +
                                                  "`address` varchar(80) DEFAULT NULL,\n"                   +
                                                  "`city` varchar(80) DEFAULT NULL,\n"                      +
                                                  "`province` varchar(80) DEFAULT NULL,\n"                  +
                                                  "`postal_code` varchar(25) DEFAULT NULL,\n"               +
                                                  "`country` smallint(3) UNSIGNED ZEROFILL DEFAULT NULL,\n" +
                                                  "`status` int NOT NULL,\n"                                +
                                                  "PRIMARY KEY (`id`),\n"                                   +
                                                  "KEY `idx_companies_name` (`name`),\n"                    +
                                                  "KEY `idx_companies_founded` (`founded`),\n"              +
                                                  "KEY `idx_companies_website` (`website`),\n"              +
                                                  "KEY `idx_companies_twitter` (`twitter`),\n"              +
                                                  "KEY `idx_companies_facebook` (`facebook`),\n"            +
                                                  "KEY `idx_companies_sold` (`sold`),\n"                    +
                                                  "KEY `idx_companies_sold_to` (`sold_to`),\n"              +
                                                  "KEY `idx_companies_address` (`address`),\n"              +
                                                  "KEY `idx_companies_city` (`city`),\n"                    +
                                                  "KEY `idx_companies_province` (`province`),\n"            +
                                                  "KEY `idx_companies_postal_code` (`postal_code`),\n"      +
                                                  "KEY `idx_companies_status` (`status`),\n"                +
                                                  "KEY `idx_companies_country` (`country`));";

        public static readonly string Computers = V6.Computers;

        public static readonly string Consoles = V6.Consoles;

        public static readonly string DiskFormats = V6.DiskFormats;

        public static readonly string Forbidden = V6.Forbidden;

        public static readonly string Gpus = V6.Gpus;

        public static readonly string Logs = V6.Logs;

        public static readonly string MoneyDonations = V6.MoneyDonations;

        public static readonly string MusicSynths = V6.MusicSynths;

        public static readonly string News = V6.News;

        public static readonly string OwnedComputers = V6.OwnedComputers;

        public static readonly string OwnedConsoles = V6.OwnedConsoles;

        public static readonly string Processors = V6.Processors;

        public static readonly string SoundSynths = V6.SoundSynths;

        public static readonly string ComputersForeignKeys = V6.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V6.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = V6.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V6.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V6.CompaniesForeignKeys;
    }
}