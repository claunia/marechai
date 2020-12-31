/******************************************************************************
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

namespace Marechai.Database.Schemas.Sql
{
    public static class V9
    {
        public static readonly string Admins = V8.Admins;

        public static readonly string BrowserTests = V8.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                  +
                                                   "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                                   "`version` int(11) NOT NULL,\n"                   +
                                                   "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                                   "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                                   "INSERT INTO marechai_db (version) VALUES ('9');";

        public static readonly string Companies = V8.Companies;

        public static readonly string Computers = V8.Computers;

        public static readonly string Consoles = V8.Consoles;

        public static readonly string DiskFormats = V8.DiskFormats;

        public static readonly string Forbidden = V8.Forbidden;

        public static readonly string Gpus = V8.Gpus;

        public static readonly string Logs = V8.Logs;

        public static readonly string MoneyDonations = V8.MoneyDonations;

        public static readonly string MusicSynths = V8.MusicSynths;

        public static readonly string News = V8.News;

        public static readonly string OwnedComputers = V8.OwnedComputers;

        public static readonly string OwnedConsoles = V8.OwnedConsoles;

        public static readonly string Processors = V8.Processors;

        public static readonly string SoundSynths = V8.SoundSynths;

        public static readonly string ComputersForeignKeys = V8.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V8.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = V8.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V8.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V8.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V8.CompanyLogos;

        public static readonly string CompanyDescriptions = "CREATE TABLE `company_descriptions` (\n" +
                                                            "`id` INT NOT NULL AUTO_INCREMENT,\n" +
                                                            "`company_id` INT NOT NULL,\n" + "`text` text,\n" +
                                                            "PRIMARY KEY (`id`),\n" +
                                                            "INDEX `idx_company_id` (`company_id` ASC),\n" +
                                                            "FULLTEXT KEY `idx_text` (`text`),\n" +
                                                            "CONSTRAINT `fk_company_id` FOREIGN KEY (`id`) REFERENCES `companies` (`id`) ON DELETE CASCADE ON UPDATE CASCADE\n" +
                                                            ")";
    }
}