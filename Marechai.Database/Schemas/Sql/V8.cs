/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V9.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 8.
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
    public static class V8
    {
        public static readonly string Admins = V7.Admins;

        public static readonly string BrowserTests = V7.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                  +
                                                   "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                                   "`version` int(11) NOT NULL,\n"                   +
                                                   "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                                   "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                                   "INSERT INTO marechai_db (version) VALUES ('8');";

        public static readonly string Companies = V7.Companies;

        public static readonly string Computers = V7.Computers;

        public static readonly string Consoles = V7.Consoles;

        public static readonly string DiskFormats = V7.DiskFormats;

        public static readonly string Forbidden = V7.Forbidden;

        public static readonly string Gpus = V7.Gpus;

        public static readonly string Logs = V7.Logs;

        public static readonly string MoneyDonations = V7.MoneyDonations;

        public static readonly string MusicSynths = V7.MusicSynths;

        public static readonly string News = V7.News;

        public static readonly string OwnedComputers = V7.OwnedComputers;

        public static readonly string OwnedConsoles = V7.OwnedConsoles;

        public static readonly string Processors = V7.Processors;

        public static readonly string SoundSynths = V7.SoundSynths;

        public static readonly string ComputersForeignKeys = V7.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V7.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = V7.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V7.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V7.CompaniesForeignKeys;

        public static readonly string CompanyLogos = "CREATE TABLE IF NOT EXISTS `company_logos` (\n"   +
                                                     "`id` INT NOT NULL AUTO_INCREMENT,\n"              +
                                                     "`company_id` INT(11) NOT NULL,\n"                 +
                                                     "`year` INT(4) DEFAULT NULL,\n"                    +
                                                     "`logo_guid` CHAR(36) NOT NULL,\n"                 +
                                                     "PRIMARY KEY (`id`, `company_id`, `logo_guid`),\n" +
                                                     "UNIQUE INDEX `idx_id` (`id` ASC),\n"              +
                                                     "INDEX `idx_company_id` (`company_id` ASC),\n"     +
                                                     "INDEX `idx_guid` (`logo_guid` ASC),\n"            +
                                                     "CONSTRAINT `fk_company_logos_company1`\n"         +
                                                     "FOREIGN KEY (`company_id`)\n"                     +
                                                     "REFERENCES `companies` (`id`))";
    }
}