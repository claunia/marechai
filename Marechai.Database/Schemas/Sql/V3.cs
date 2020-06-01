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
    public static class V3
    {
        public static readonly string Admins = @"CREATE TABLE `admins` (
                                                `id` int(11) NOT NULL AUTO_INCREMENT,
                                                `user` char(50) NOT NULL DEFAULT '',
                                                `password` char(50) CHARACTER SET latin1 COLLATE latin1_bin NOT NULL DEFAULT '',
                                                 PRIMARY KEY (`id`)
                                                );";

        public static readonly string BrowserTests = @"CREATE TABLE IF NOT EXISTS `browser_tests` (
                                                      `id` smallint(5) unsigned zerofill NOT NULL AUTO_INCREMENT,
                                                      `user_agent` varchar(128) NOT NULL DEFAULT '',
                                                      `browser` varchar(64) NOT NULL DEFAULT '',
                                                      `version` varchar(16) NOT NULL DEFAULT '',
                                                      `os` varchar(32) NOT NULL DEFAULT '',
                                                      `platform` varchar(8) NOT NULL DEFAULT '',
                                                      `gif87` tinyint(1) NOT NULL DEFAULT '0',
                                                      `gif89` tinyint(1) NOT NULL DEFAULT '0',
                                                      `jpeg` tinyint(1) NOT NULL DEFAULT '0',
                                                      `png` tinyint(1) NOT NULL DEFAULT '0',
                                                      `pngt` tinyint(1) NOT NULL DEFAULT '0',
                                                      `agif` tinyint(1) NOT NULL DEFAULT '0',
                                                      `table` tinyint(1) NOT NULL DEFAULT '0',
                                                      `colors` tinyint(1) NOT NULL DEFAULT '0',
                                                      `js` tinyint(1) NOT NULL DEFAULT '0',
                                                      `frames` tinyint(1) NOT NULL DEFAULT '0',
                                                      `flash` tinyint(1) NOT NULL DEFAULT '0',
                                                       PRIMARY KEY (`id`)
                                                       );";

        public static readonly string MarechaiDb = @"CREATE TABLE `marechai_db` (
                                                `id` int(11) NOT NULL AUTO_INCREMENT,
                                                `version` int(11) NOT NULL,
                                                `updated` datetime DEFAULT CURRENT_TIMESTAMP,
                                                 PRIMARY KEY (`id`)
                                                 );
                                                 INSERT INTO marechai_db (version) VALUES ('3');";

        public static readonly string Companies = @"CREATE TABLE IF NOT EXISTS `companies` (
                                                   `id` int(11) NOT NULL AUTO_INCREMENT,
                                                   `name` char(128) NOT NULL DEFAULT '',
                                                    PRIMARY KEY (`id`)
                                                   );";

        public static readonly string Computers = @"CREATE TABLE IF NOT EXISTS `computers` (
                                                   `id` int(11) NOT NULL AUTO_INCREMENT,
                                                   `company` int(11) NOT NULL DEFAULT '0',
                                                   `year` int(11) NOT NULL DEFAULT '0',
                                                   `model` char(50) NOT NULL DEFAULT '',
                                                   `cpu1` int(11) NOT NULL DEFAULT '0',
                                                   `mhz1` decimal(11,2) NOT NULL DEFAULT '0.00',
                                                   `cpu2` int(11) DEFAULT NULL,
                                                   `mhz2` decimal(11,2) DEFAULT NULL,
                                                   `bits` int(11) NOT NULL DEFAULT '0',
                                                   `ram` int(11) NOT NULL DEFAULT '0',
                                                   `rom` int(11) NOT NULL DEFAULT '0',
                                                   `gpu` int(11) NOT NULL DEFAULT '0',
                                                   `vram` int(11) NOT NULL DEFAULT '0',
                                                   `colors` int(11) NOT NULL DEFAULT '0',
                                                   `res` char(10) NOT NULL DEFAULT '',
                                                   `sound_synth` int(11) NOT NULL DEFAULT '0',
                                                   `music_synth` int(11) NOT NULL DEFAULT '0',
                                                   `sound_channels` int(11) NOT NULL DEFAULT '0',
                                                   `music_channels` int(11) NOT NULL DEFAULT '0',
                                                   `hdd1` int(11) NOT NULL DEFAULT '0',
                                                   `hdd2` int(11) DEFAULT NULL,
                                                   `hdd3` int(11) DEFAULT NULL,
                                                   `disk1` int(11) NOT NULL DEFAULT '0',
                                                   `cap1` char(25) NOT NULL DEFAULT '0',
                                                   `disk2` int(11) DEFAULT NULL,
                                                   `cap2` char(25) DEFAULT NULL,
                                                   `comment` char(255) DEFAULT NULL,
                                                    PRIMARY KEY (`id`)
                                                   );";

        public static readonly string Consoles = @"CREATE TABLE IF NOT EXISTS `consoles` (
                                                  `id` int(11) NOT NULL AUTO_INCREMENT,
                                                  `company` int(11) NOT NULL DEFAULT '0',
                                                  `model` char(50) NOT NULL DEFAULT '',
                                                  `year` int(11) NOT NULL DEFAULT '0',
                                                  `cpu1` int(11) NOT NULL DEFAULT '0',
                                                  `mhz1` decimal(11,2) NOT NULL DEFAULT '0.00',
                                                  `cpu2` int(11) DEFAULT NULL,
                                                  `mhz2` decimal(11,2) DEFAULT NULL,
                                                  `bits` int(11) NOT NULL DEFAULT '0',
                                                  `ram` int(11) NOT NULL DEFAULT '0',
                                                  `rom` int(11) NOT NULL DEFAULT '0',
                                                  `gpu` int(11) NOT NULL DEFAULT '0',
                                                  `vram` int(11) NOT NULL DEFAULT '0',
                                                  `res` char(11) NOT NULL DEFAULT '',
                                                  `colors` int(11) NOT NULL DEFAULT '0',
                                                  `palette` int(11) NOT NULL DEFAULT '0',
                                                  `sound_synth` int(11) NOT NULL DEFAULT '0',
                                                  `schannels` int(11) NOT NULL DEFAULT '0',
                                                  `music_channels` int(11) NOT NULL DEFAULT '0',
                                                  `mchannels` int(11) NOT NULL DEFAULT '0',
                                                  `format` int(11) NOT NULL DEFAULT '0',
                                                  `cap` int(11) NOT NULL DEFAULT '0',
                                                  `comments` char(255) NOT NULL DEFAULT '',
                                                   PRIMARY KEY (`id`)
                                                   );";

        public static readonly string DiskFormats = @"CREATE TABLE IF NOT EXISTS `disk_formats` (
                                                     `id` int(11) NOT NULL AUTO_INCREMENT,
                                                     `description` char(50) NOT NULL DEFAULT '',
                                                      PRIMARY KEY (`id`)
                                                      );";

        public static readonly string Forbidden = @"CREATE TABLE IF NOT EXISTS `forbidden` (
                                                   `id` int(11) NOT NULL AUTO_INCREMENT,
                                                   `browser` char(128) NOT NULL DEFAULT '',
                                                   `date` char(20) NOT NULL DEFAULT '',
                                                   `ip` char(16) NOT NULL DEFAULT '',
                                                   `referer` char(255) NOT NULL DEFAULT '',
                                                    PRIMARY KEY (`id`)
                                                   );";

        public static readonly string Gpus = @"CREATE TABLE IF NOT EXISTS `gpus` (
                                              `id` int(11) NOT NULL AUTO_INCREMENT,
                                              `name` char(128) NOT NULL DEFAULT '',
                                               PRIMARY KEY (`id`)
                                               );";

        public static readonly string Logs = @"CREATE TABLE IF NOT EXISTS `log` (
                                              `id` int(11) NOT NULL AUTO_INCREMENT,
                                              `browser` char(128) NOT NULL DEFAULT '',
                                              `ip` char(16) NOT NULL DEFAULT '',
                                              `date` char(20) NOT NULL DEFAULT '',
                                              `referer` char(255) NOT NULL DEFAULT '',
                                               PRIMARY KEY (`id`)
                                               );";

        public static readonly string MoneyDonations = @"CREATE TABLE IF NOT EXISTS `money_donation` (
                                                        `id` int(11) NOT NULL AUTO_INCREMENT,
                                                        `donator` char(128) NOT NULL DEFAULT '',
                                                        `quantity` decimal(11,2) NOT NULL DEFAULT '0.00',
                                                         PRIMARY KEY (`id`)
                                                         );";

        public static readonly string MusicSynths = @"CREATE TABLE IF NOT EXISTS `music_synths` (
                                                     `id` int(11) NOT NULL AUTO_INCREMENT,
                                                     `name` char(50) NOT NULL DEFAULT '',
                                                      PRIMARY KEY (`id`)
                                                      );";

        public static readonly string News = @"CREATE TABLE IF NOT EXISTS `news` (
                                              `id` int(11) NOT NULL AUTO_INCREMENT,
                                              `date` char(20) NOT NULL DEFAULT '',
                                              `type` int(11) NOT NULL DEFAULT '0',
                                              `added_id` int(11) NOT NULL DEFAULT '0',
                                               PRIMARY KEY (`id`)
                                               );";

        public static readonly string OwnedComputers = @"CREATE TABLE IF NOT EXISTS `owned_computers` (
                                                        `id` int(11) NOT NULL AUTO_INCREMENT,
                                                        `db_id` int(11) NOT NULL DEFAULT '0',
                                                        `date` varchar(20) NOT NULL DEFAULT '',
                                                        `status` int(11) NOT NULL DEFAULT '0',
                                                        `trade` int(11) NOT NULL DEFAULT '0',
                                                        `boxed` int(11) NOT NULL DEFAULT '0',
                                                        `manuals` int(11) NOT NULL DEFAULT '0',
                                                        `cpu1` int(11) NOT NULL DEFAULT '0',
                                                        `mhz1` decimal(10,0) NOT NULL DEFAULT '0',
                                                        `cpu2` int(11) NOT NULL DEFAULT '0',
                                                        `mhz2` decimal(10,0) NOT NULL DEFAULT '0',
                                                        `ram` int(11) NOT NULL DEFAULT '0',
                                                        `vram` int(11) NOT NULL DEFAULT '0',
                                                        `rigid` varchar(64) NOT NULL DEFAULT '',
                                                        `disk1` int(11) NOT NULL DEFAULT '0',
                                                        `cap1` int(11) NOT NULL DEFAULT '0',
                                                        `disk2` int(11) NOT NULL DEFAULT '0',
                                                        `cap2` int(11) NOT NULL DEFAULT '0',
                                                         PRIMARY KEY (`id`)
                                                         );";

        public static readonly string OwnedConsoles = @"CREATE TABLE IF NOT EXISTS `owned_consoles` (
                                                       `id` int(11) NOT NULL AUTO_INCREMENT,
                                                       `db_id` int(11) NOT NULL DEFAULT '0',
                                                       `date` char(20) NOT NULL DEFAULT '',
                                                       `status` int(11) NOT NULL DEFAULT '0',
                                                       `trade` int(11) NOT NULL DEFAULT '0',
                                                       `boxed` int(11) NOT NULL DEFAULT '0',
                                                       `manuals` int(11) NOT NULL DEFAULT '0',
                                                        PRIMARY KEY (`id`)
                                                        );";

        public static readonly string Processors = @"CREATE TABLE IF NOT EXISTS `processors` (
                                                    `id` int(11) NOT NULL AUTO_INCREMENT,
                                                    `name` char(50) NOT NULL DEFAULT '',
                                                     KEY `id` (`id`)
                                                     );";

        public static readonly string SoundSynths = @"CREATE TABLE IF NOT EXISTS `sound_synths` (
                                                     `id` int(11) NOT NULL AUTO_INCREMENT,
                                                     `name` char(50) NOT NULL DEFAULT '',
                                                      PRIMARY KEY (`id`)
                                                      );";
    }
}