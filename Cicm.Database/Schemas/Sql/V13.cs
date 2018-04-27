/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : V12.cs
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

namespace Cicm.Database.Schemas.Sql
{
    public static class V13
    {
        public static readonly string Admins = V12.Admins;

        public static readonly string BrowserTests = V12.BrowserTests;

        public static readonly string CicmDb = "CREATE TABLE `cicm_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                               "INSERT INTO cicm_db (version) VALUES ('13');";

        public static readonly string Companies = V12.Companies;

        public static readonly string Computers = V12.Computers;

        public static readonly string Consoles = V12.Consoles;

        public static readonly string DiskFormats = V12.DiskFormats;

        public static readonly string Forbidden = V12.Forbidden;

        public static readonly string Gpus = V12.Gpus;

        public static readonly string Logs = V12.Logs;

        public static readonly string MoneyDonations = V12.MoneyDonations;

        public static readonly string MusicSynths = V12.MusicSynths;

        public static readonly string News = V12.News;

        public static readonly string OwnedComputers = V12.OwnedComputers;

        public static readonly string OwnedConsoles = V12.OwnedConsoles;

        public static readonly string Processors = V12.Processors;

        public static readonly string SoundSynths = V12.SoundSynths;

        public static readonly string ComputersForeignKeys = V12.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V12.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = V12.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V12.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V12.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V12.CompanyLogos;

        public static readonly string CompanyDescriptions = V12.CompanyDescriptions;

        public static readonly string InstructionSets = V12.InstructionSets;

        public static readonly string InstructionSetExtensions = V12.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V12.InstructionSetExtensionsByProcessor;
    }
}