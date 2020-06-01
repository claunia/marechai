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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

namespace Marechai.Database.Schemas.Sql
{
    public static class V4
    {
        public static readonly string Admins = V3.Admins + "\n" + "CREATE INDEX idx_admins_user ON admins (user);";

        public static readonly string BrowserTests =
            V3.BrowserTests                                                              + "\n" +
            "CREATE INDEX idx_browser_tests_user_agent ON browser_tests (user_agent);\n" +
            "CREATE INDEX idx_browser_tests_browser ON browser_tests (browser);\n"       +
            "CREATE INDEX idx_browser_tests_version ON browser_tests (version);\n"       +
            "CREATE INDEX idx_browser_tests_os ON browser_tests (os);\n"                 +
            "CREATE INDEX idx_browser_tests_platform ON browser_tests (platform);";

        public static readonly string MarechaiDb = @"CREATE TABLE `marechai_db` (
                                                `id` int(11) NOT NULL AUTO_INCREMENT,
                                                `version` int(11) NOT NULL,
                                                `updated` datetime DEFAULT CURRENT_TIMESTAMP,
                                                 PRIMARY KEY (`id`)
                                                 );
                                                 INSERT INTO marechai_db (version) VALUES ('4');";

        public static readonly string Companies =
            V3.Companies + "\n" + "CREATE INDEX idx_companies_name ON companies (name);";

        public static readonly string Computers =
            V3.Computers                                                           + "\n" +
            "CREATE INDEX idx_computers_company ON computers (company);\n"         +
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

        public static readonly string Consoles = V3.Consoles                                                          +
                                                 "\n"                                                                 +
                                                 "CREATE INDEX idx_consoles_company ON consoles (company);\n"         +
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

        public static readonly string DiskFormats = V3.DiskFormats + "\n" +
                                                    "CREATE INDEX idx_disk_formats_description ON disk_formats (description);";

        public static readonly string Forbidden = V3.Forbidden                                                   +
                                                  "\n"                                                           +
                                                  "CREATE INDEX idx_forbidden_browser ON forbidden (browser);\n" +
                                                  "CREATE INDEX idx_forbidden_date ON forbidden (date);\n"       +
                                                  "CREATE INDEX idx_forbidden_ip ON forbidden (ip);\n"           +
                                                  "CREATE INDEX idx_forbidden_referer ON forbidden (referer);";

        public static readonly string Gpus = V3.Gpus + "\n" + "CREATE INDEX idx_gpus_name ON gpus (name);";

        public static readonly string Logs = V3.Logs                                            + "\n" +
                                             "CREATE INDEX idx_log_browser ON log (browser);\n" +
                                             "CREATE INDEX idx_log_date ON log (date);\n"       +
                                             "CREATE INDEX idx_log_ip ON log (ip);\n"           +
                                             "CREATE INDEX idx_log_referer ON log (referer);";

        public static readonly string MoneyDonations =
            V3.MoneyDonations                                                          + "\n" +
            "CREATE INDEX idx_money_donations_donator ON money_donations (donator);\n" +
            "CREATE INDEX idx_money_donations_quantity ON money_donations (quantity);";

        public static readonly string MusicSynths =
            V3.MusicSynths + "\n" + "CREATE INDEX idx_music_synts_name ON music_synths (name);";

        public static readonly string News = V3.News                                        + "\n" +
                                             "CREATE INDEX idx_news_date ON news (date);\n" +
                                             "CREATE INDEX idx_news_type ON news (type);\n" +
                                             "CREATE INDEX idx_news_ip ON news (added_id);";

        public static readonly string OwnedComputers =
            V3.OwnedComputers                                                          + "\n" +
            "CREATE INDEX idx_owned_computers_db_id ON owned_computers (db_id);\n"     +
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

        public static readonly string OwnedConsoles =
            V3.OwnedConsoles                                                        + "\n" +
            "CREATE INDEX idx_owned_consoles_db_id ON owned_consoles (db_id);\n"    +
            "CREATE INDEX idx_owned_consoles_date    ON owned_consoles (date);\n"   +
            "CREATE INDEX idx_owned_consoles_status  ON owned_consoles (status);\n" +
            "CREATE INDEX idx_owned_consoles_trade   ON owned_consoles (trade);\n"  +
            "CREATE INDEX idx_owned_consoles_boxed   ON owned_consoles (boxed);\n"  +
            "CREATE INDEX idx_owned_consoles_manuals ON owned_consoles (manuals);";

        public static readonly string Processors = @"CREATE TABLE IF NOT EXISTS `processors` (
                                                    `id` int(11) NOT NULL AUTO_INCREMENT,
                                                    `name` char(50) NOT NULL DEFAULT '',
                                                     PRIMARY KEY `id` (`id`)
                                                     );" + "\n" +
                                                   "CREATE INDEX idx_processors_name ON processors (name);";

        public static readonly string SoundSynths =
            V3.SoundSynths + "\n" + "CREATE INDEX idx_sound_synths_name ON sound_synths (name);";
    }
}