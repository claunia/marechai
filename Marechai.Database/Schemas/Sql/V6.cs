/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V6.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 6.
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
    public static class V6
    {
        public static readonly string Admins = V5.Admins;

        public static readonly string BrowserTests = V5.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                               "INSERT INTO marechai_db (version) VALUES ('6');";

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
                                                  "KEY `idx_companies_country` (`country`));";

        public static readonly string Computers = V5.Computers;

        public static readonly string Consoles = V5.Consoles;

        public static readonly string DiskFormats = V5.DiskFormats;

        public static readonly string Forbidden = V5.Forbidden;

        public static readonly string Gpus = V5.Gpus;

        public static readonly string Logs = V5.Logs;

        public static readonly string MoneyDonations = V5.MoneyDonations;

        public static readonly string MusicSynths = V5.MusicSynths;

        public static readonly string News = V5.News;

        public static readonly string OwnedComputers = V5.OwnedComputers;

        public static readonly string OwnedConsoles = V5.OwnedConsoles;

        public static readonly string Processors = V5.Processors;

        public static readonly string SoundSynths = V5.SoundSynths;

        public static readonly string ComputersForeignKeys = V5.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V5.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = "CREATE TABLE `iso3166_1_numeric` (\n"           +
                                                       "`id` SMALLINT(3) UNSIGNED ZEROFILL NOT NULL,\n" +
                                                       "`name` VARCHAR(64) NOT NULL,\n"                 +
                                                       "PRIMARY KEY (`id`),\n"                          +
                                                       "INDEX `idx_name` (`name` ASC));";

        public static readonly string Iso3166NumericValues =
            "INSERT INTO `iso3166_1_numeric` VALUES (004,'Afghanistan'),(248,'Åland Islands'),(008,'Albania'),"            +
            "(012,'Algeria'),(016,'American Samoa'),(020,'Andorra'),(024,'Angola'),(660,'Anguilla'),(010,'Antarctica'),"   +
            "(028,'Antigua and Barbuda'),(032,'Argentina'),(051,'Armenia'),(533,'Aruba'),(036,'Australia'),"               +
            "(040,'Austria'),(031,'Azerbaijan'),(044,'Bahamas'),(048,'Bahrain'),(050,'Bangladesh'),(052,'Barbados'),"      +
            "(112,'Belarus'),(056,'Belgium'),(084,'Belize'),(204,'Benin'),(060,'Bermuda'),(064,'Bhutan'),"                 +
            "(862,'Bolivarian Republic of Venezuela'),(535,'Bonaire, Sint Eustatius and Saba'),"                           +
            "(070,'Bosnia and Herzegovina'),(072,'Botswana'),(074,'Bouvet Island'),(076,'Brazil'),"                        +
            "(080,'British Antarctic Territory'),(086,'British Indian Ocean Territory'),(092,'British Virgin Islands'),"   +
            "(096,'Brunei Darussalam'),(100,'Bulgaria'),(854,'Burkina Faso'),(108,'Burundi'),(132,'Cabo Verde'),"          +
            "(116,'Cambodia'),(120,'Cameroon'),(124,'Canada'),(128,'Canton and Enderbury Islands'),"                       +
            "(136,'Cayman Islands'),(140,'Central African Republic'),(148,'Chad'),(830,'Channel Islands'),(152,'Chile'),"  +
            "(156,'China'),(162,'Christmas Island'),(166,'Cocos (Keeling) Islands'),(170,'Colombia'),(174,'Comoros'),"     +
            "(178,'Congo'),(184,'Cook Islands'),(188,'Costa Rica'),(384,'Côte d\\'Ivoire'),(191,'Croatia'),(192,'Cuba'),"  +
            "(531,'Curaçao'),(196,'Cyprus'),(203,'Czechia'),(200,'Czechoslovakia'),"                                       +
            "(408,'Democratic People\\'s Republic of Korea'),(180,'Democratic Republic of the Congo'),"                    +
            "(720,'Democratic Yemen'),(208,'Denmark'),(262,'Djibouti'),(212,'Dominica'),(214,'Dominican Republic'),"       +
            "(216,'Dronning Maud Land'),(218,'Ecuador'),(818,'Egypt'),(222,'El Salvador'),(226,'Equatorial Guinea'),"      +
            "(232,'Eritrea'),(233,'Estonia'),(230,'Ethiopia'),(231,'Ethiopia'),(238,'Falkland Islands (Malvinas)'),"       +
            "(234,'Faroe Islands'),(280,'Federal Republic of Germany'),(583,'Federated States of Micronesia'),"            +
            "(242,'Fiji'),(246,'Finland'),(250,'France'),(249,'France, Metropolitan'),(254,'French Guiana'),"              +
            "(258,'French Polynesia'),(260,'French Southern Territories'),(266,'Gabon'),(270,'Gambia'),"                   +
            "(274,'Gaza Strip (Palestine)'),(268,'Georgia'),(278,'German Democratic Republic'),(276,'Germany'),"           +
            "(288,'Ghana'),(292,'Gibraltar'),(300,'Greece'),(304,'Greenland'),(308,'Grenada'),(312,'Guadeloupe'),"         +
            "(316,'Guam'),(320,'Guatemala'),(831,'Guernsey'),(324,'Guinea'),(624,'Guinea-Bissau'),(328,'Guyana'),"         +
            "(332,'Haiti'),(334,'Heard Island and McDonald Islands'),(336,'Holy See'),(340,'Honduras'),"                   +
            "(344,'Hong Kong'),(348,'Hungary'),(352,'Iceland'),(356,'India'),(360,'Indonesia'),(368,'Iraq'),"              +
            "(372,'Ireland'),(364,'Islamic Republic of Iran'),(833,'Isle of Man'),(376,'Israel'),(380,'Italy'),"           +
            "(388,'Jamaica'),(392,'Japan'),(832,'Jersey'),(396,'Johnston Island'),(400,'Jordan'),(398,'Kazakhstan'),"      +
            "(404,'Kenya'),(296,'Kiribati'),(414,'Kuwait'),(417,'Kyrgyzstan'),(418,'Lao People\\'s Democratic Republic')," +
            "(428,'Latvia'),(422,'Lebanon'),(426,'Lesotho'),(430,'Liberia'),(434,'Libya'),(438,'Liechtenstein'),"          +
            "(440,'Lithuania'),(442,'Luxembourg'),(446,'Macao'),(450,'Madagascar'),(454,'Malawi'),(458,'Malaysia'),"       +
            "(462,'Maldives'),(466,'Mali'),(470,'Malta'),(584,'Marshall Islands'),(474,'Martinique'),(478,'Mauritania'),"  +
            "(480,'Mauritius'),(175,'Mayotte'),(484,'Mexico'),(488,'Midway Islands'),(492,'Monaco'),(496,'Mongolia'),"     +
            "(499,'Montenegro'),(500,'Montserrat'),(504,'Morocco'),(508,'Mozambique'),(104,'Myanmar'),(516,'Namibia'),"    +
            "(520,'Nauru'),(524,'Nepal'),(528,'Netherlands'),(530,'Netherlands Antilles'),(532,'Netherlands Antilles'),"   +
            "(536,'Neutral Zone'),(540,'New Caledonia'),(554,'New Zealand'),(558,'Nicaragua'),(562,'Niger'),"              +
            "(566,'Nigeria'),(570,'Niue'),(574,'Norfolk Island'),(580,'Northern Mariana Islands'),(578,'Norway'),"         +
            "(512,'Oman'),(586,'Pakistan'),(585,'Palau'),(590,'Panama'),(591,'Panama'),(594,'Panama Canal Zone'),"         +
            "(598,'Papua New Guinea'),(600,'Paraguay'),(604,'Peru'),(608,'Philippines'),(612,'Pitcairn'),"                 +
            "(068,'Plurinational State of Bolivia'),(616,'Poland'),(620,'Portugal'),(630,'Puerto Rico'),(634,'Qatar'),"    +
            "(410,'Republic of Korea'),(498,'Republic of Moldova'),(714,'Republic of Viet-Nam'),(638,'Réunion'),"          +
            "(642,'Romania'),(643,'Russian Federation'),(646,'Rwanda'),(650,'Ryukyu Islands'),(652,'Saint Barthélemy'),"   +
            "(654,'Saint Helena, Ascension and Tristan da Cunha'),(659,'Saint Kitts and Nevis'),"                          +
            "(658,'Saint Kitts-Nevis-Anguilla'),(662,'Saint Lucia'),(663,'Saint Martin'),"                                 +
            "(666,'Saint Pierre and Miquelon'),(670,'Saint Vincent and the Grenadines'),(882,'Samoa'),"                    +
            "(674,'San Marino'),(678,'Sao Tome and Principe'),(682,'Saudi Arabia'),(686,'Senegal'),(688,'Serbia'),"        +
            "(891,'Serbia and Montenegro'),(690,'Seychelles'),(694,'Sierra Leone'),(698,'Sikkim'),(702,'Singapore'),"      +
            "(534,'Sint Marteen'),(703,'Slovakia'),(705,'Slovenia'),(890,'Socialist Federal Republic of Yugoslavia'),"     +
            "(090,'Solomon Islands'),(706,'Somalia'),(710,'South Africa'),"                                                +
            "(239,'South Georgia and the South Sandwich Islands'),(728,'South Sudan'),(724,'Spain'),(144,'Sri Lanka'),"    +
            "(275,'State of Palestine'),(729,'Sudan'),(736,'Sudan'),(740,'Suriname'),(744,'Svalbard and Jan Mayen'),"      +
            "(748,'Swaziland'),(752,'Sweden'),(756,'Switzerland'),(760,'Syrian Arab Republic'),"                           +
            "(158,'Taiwan, Province of China'),(762,'Tajikistan'),(764,'Thailand'),"                                       +
            "(807,'The former Yugoslav Republic of Macedonia'),(626,'Timor-Leste'),(768,'Togo'),(772,'Tokelau'),"          +
            "(776,'Tonga'),(780,'Trinidad and Tobago'),(582,'Trust Territory of the Pacific Islands'),(788,'Tunisia'),"    +
            "(792,'Turkey'),(795,'Turkmenistan'),(796,'Turks and Caicos Islands'),(798,'Tuvalu'),"                         +
            "(849,'U.S. Miscellaneous Pacific Islands'),(800,'Uganda'),(804,'Ukraine'),(784,'United Arab Emirates'),"      +
            "(826,'United Kingdom'),(834,'United Republic of Tanzania'),(581,'United States Minor Outlying Islands'),"     +
            "(840,'United States of America'),(858,'Uruguay'),(810,'USSR'),(860,'Uzbekistan'),(548,'Vanuatu'),"            +
            "(704,'Viet-Nam'),(850,'Virgin Islands, U.S.'),(872,'Wake Island'),(876,'Wallis and Futuna'),"                 +
            "(732,'Western Sahara'),(887,'Yemen'),(886,'Yemen Arab Republic'),(894,'Zambia'),(716,'Zimbabwe');";

        public static readonly string CompaniesForeignKeys =
            "ALTER TABLE `companies` ADD FOREIGN KEY `fk_companies_sold_to` (sold_to) REFERENCES `companies` (`id`);\n" +
            "ALTER TABLE `companies` ADD FOREIGN KEY `fk_companies_country` (country) REFERENCES `iso3166_1_numeric` (`id`);";
    }
}