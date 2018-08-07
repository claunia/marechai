/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : V5.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 5.
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
    public static class V5
    {
        public static readonly string Admins = V4.Admins;

        public static readonly string BrowserTests = V4.BrowserTests;

        public static readonly string CicmDb = @"CREATE TABLE `cicm_db` (
                                                `id` int(11) NOT NULL AUTO_INCREMENT,
                                                `version` int(11) NOT NULL,
                                                `updated` datetime DEFAULT CURRENT_TIMESTAMP,
                                                 PRIMARY KEY (`id`)
                                                 );
                                                 INSERT INTO cicm_db (version) VALUES ('5');";

        public static readonly string Companies = V4.Companies;

        public static readonly string Computers = V4.Computers;

        public static readonly string Consoles = V4.Consoles;

        public static readonly string DiskFormats = V4.DiskFormats;

        public static readonly string Forbidden = V4.Forbidden;

        public static readonly string Gpus = V4.Gpus;

        public static readonly string Logs = V4.Logs;

        public static readonly string MoneyDonations = V4.MoneyDonations;

        public static readonly string MusicSynths = V4.MusicSynths;

        public static readonly string News = V4.News;

        public static readonly string OwnedComputers = V4.OwnedComputers;

        public static readonly string OwnedConsoles = V4.OwnedConsoles;

        public static readonly string Processors = V4.Processors;

        public static readonly string SoundSynths = V4.SoundSynths;

        public static readonly string ComputersForeignKeys =
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_company (company) REFERENCES companies (id);\n"            +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_cpu1 (cpu1) REFERENCES               processors (id);\n"   +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_cpu2 (cpu2) REFERENCES               processors (id);\n"   +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_gpu (gpu) REFERENCES                 gpus (id);\n"         +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_sound_synth (sound_synth) REFERENCES sound_synths (id);\n" +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_music_synth (music_synth) REFERENCES music_synths (id);\n" +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_hdd1 (hdd1) REFERENCES               disk_formats (id);\n" +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_hdd2 (hdd2) REFERENCES               disk_formats (id);\n" +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_hdd3 (hdd3) REFERENCES               disk_formats (id);\n" +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_disk1 (disk1) REFERENCES             disk_formats (id);\n" +
            "ALTER TABLE computers ADD FOREIGN KEY fk_computers_disk2 (disk2) REFERENCES disk_formats (id);";

        public static readonly string ConsolesForeignKeys =
            "ALTER TABLE consoles ADD FOREIGN KEY fk_consoles_company (company) REFERENCES companies (id);\n"            +
            "ALTER TABLE consoles ADD FOREIGN KEY fk_consoles_cpu1 (cpu1) REFERENCES               processors (id);\n"   +
            "ALTER TABLE consoles ADD FOREIGN KEY fk_consoles_cpu2 (cpu2) REFERENCES               processors (id);\n"   +
            "ALTER TABLE consoles ADD FOREIGN KEY fk_consoles_gpu (gpu) REFERENCES                 gpus (id);\n"         +
            "ALTER TABLE consoles ADD FOREIGN KEY fk_consoles_sound_synth (sound_synth) REFERENCES sound_synths (id);\n" +
            "ALTER TABLE consoles ADD FOREIGN KEY fk_consoles_music_synth (music_synth) REFERENCES music_synths (id);\n" +
            "ALTER TABLE consoles ADD FOREIGN KEY fk_consoles_format (format) REFERENCES disk_formats (id);";
    }
}