namespace Cicm.Database.Schemas.Sql
{
    public static class V2
    {
        static readonly string Admins = @"CREATE TABLE IF NOT EXISTS `admin` ( 
                               `id` int(11)    NOT NULL AUTO_INCREMENT, 
                               `user` char(50) NOT NULL      DEFAULT '', 
                               `password` char(50) CHARACTER SET latin1 COLLATE latin1_bin NOT NULL DEFAULT '', 
                               PRIMARY             KEY (`id`) 
                               );";

        static readonly string BrowserTests = @"CREATE TABLE IF NOT EXISTS `browser_test` (
                                               `id` smallint(5) unsigned zerofill NOT NULL AUTO_INCREMENT,
                                               `idstring` varchar(128) NOT NULL DEFAULT '',
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

        static readonly string Companies = @"CREATE TABLE IF NOT EXISTS `Companias` (
                                            `id` int(11) NOT NULL AUTO_INCREMENT,
                                            `Compania` char(128) NOT NULL DEFAULT '',
                                             PRIMARY KEY (`id`)
                                            );";

        static readonly string Computers = @"CREATE TABLE IF NOT EXISTS `computers` (
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
                                            `spu` int(11) NOT NULL DEFAULT '0',
                                            `mpu` int(11) NOT NULL DEFAULT '0',
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

        static readonly string Consoles = @"CREATE TABLE IF NOT EXISTS `consoles` (
                                           `id` int(11) NOT NULL AUTO_INCREMENT,
                                           `company` int(11) NOT NULL DEFAULT '0',
                                           `name` char(50) NOT NULL DEFAULT '',
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
                                           `spu` int(11) NOT NULL DEFAULT '0',
                                           `schannels` int(11) NOT NULL DEFAULT '0',
                                           `mpu` int(11) NOT NULL DEFAULT '0',
                                           `mchannels` int(11) NOT NULL DEFAULT '0',
                                           `format` int(11) NOT NULL DEFAULT '0',
                                           `cap` int(11) NOT NULL DEFAULT '0',
                                           `comments` char(255) NOT NULL DEFAULT '',
                                            PRIMARY KEY (`id`)
                                            );";

        static readonly string ConsoleCompanies = @"CREATE TABLE IF NOT EXISTS `console_company` (
                                                   `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
                                                   `company` char(128) NOT NULL DEFAULT '',
                                                    PRIMARY KEY (`id`)
                                                    );";

        static readonly string Cpus = @"CREATE TABLE IF NOT EXISTS `cpu` (
                                        `id` int(11) NOT NULL AUTO_INCREMENT,
                                        `cpu` char(50) NOT NULL DEFAULT '',
                                         KEY `id` (`id`)
                                         );";

        static readonly string Dsps = @"CREATE TABLE IF NOT EXISTS `DSPs` (
                                       `id` int(11) NOT NULL AUTO_INCREMENT,
                                       `DSP` char(50) NOT NULL DEFAULT '',
                                        PRIMARY KEY (`id`)
                                        );";

        static readonly string Forbidden = @"CREATE TABLE IF NOT EXISTS `forbidden` (
                                            `id` int(11) NOT NULL AUTO_INCREMENT,
                                            `browser` char(128) NOT NULL DEFAULT '',
                                            `date` char(20) NOT NULL DEFAULT '',
                                            `ip` char(16) NOT NULL DEFAULT '',
                                            `referer` char(255) NOT NULL DEFAULT '',
                                             PRIMARY KEY (`id`)
                                            );";

        static readonly string DiskFormats = @"CREATE TABLE IF NOT EXISTS `Formatos_de_disco` (
                                              `id` int(11) NOT NULL AUTO_INCREMENT,
                                              `Format` char(50) NOT NULL DEFAULT '',
                                               PRIMARY KEY (`id`)
                                               );";

        static readonly string Gpus = @"CREATE TABLE IF NOT EXISTS `gpus` (
                                       `id` int(11) NOT NULL AUTO_INCREMENT,
                                       `gpu` char(128) NOT NULL DEFAULT '',
                                        PRIMARY KEY (`id`)
                                        );";

        static readonly string Logs = @"CREATE TABLE IF NOT EXISTS `log` (
                                       `id` int(11) NOT NULL AUTO_INCREMENT,
                                       `browser` char(128) NOT NULL DEFAULT '',
                                       `ip` char(16) NOT NULL DEFAULT '',
                                       `date` char(20) NOT NULL DEFAULT '',
                                       `referer` char(255) NOT NULL DEFAULT '',
                                        PRIMARY KEY (`id`)
                                        );";

        static readonly string MoneyDonations = @"CREATE TABLE IF NOT EXISTS `money_donation` (
                                                 `id` int(11) NOT NULL AUTO_INCREMENT,
                                                 `donator` char(128) NOT NULL DEFAULT '',
                                                 `quantity` decimal(11,2) NOT NULL DEFAULT '0.00',
                                                  PRIMARY KEY (`id`)
                                                  );";

        static readonly string Mpus = @"CREATE TABLE IF NOT EXISTS `mpus` (
                                       `id` int(11) NOT NULL AUTO_INCREMENT,
                                       `mpu` char(50) NOT NULL DEFAULT '',
                                        PRIMARY KEY (`id`)
                                        );";

        static readonly string News = @"CREATE TABLE IF NOT EXISTS `news` (
                                       `id` int(11) NOT NULL AUTO_INCREMENT,
                                       `date` char(20) NOT NULL DEFAULT '',
                                       `type` int(11) NOT NULL DEFAULT '0',
                                       `added_id` int(11) NOT NULL DEFAULT '0',
                                        PRIMARY KEY (`id`)
                                        );";

        static readonly string OwnComputers = @"CREATE TABLE IF NOT EXISTS `own_computer` (
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

        static readonly string OwnConsoles = @"CREATE TABLE IF NOT EXISTS `own_consoles` (
                                              `id` int(11) NOT NULL AUTO_INCREMENT,
                                              `db_id` int(11) NOT NULL DEFAULT '0',
                                              `date` char(20) NOT NULL DEFAULT '',
                                              `status` int(11) NOT NULL DEFAULT '0',
                                              `trade` int(11) NOT NULL DEFAULT '0',
                                              `boxed` int(11) NOT NULL DEFAULT '0',
                                              `manuals` int(11) NOT NULL DEFAULT '0',
                                               PRIMARY KEY (`id`)
                                               );";

        static readonly string ProcesadoresPrincipales = @"CREATE TABLE IF NOT EXISTS `procesadores_principales` (
                                                          `id` int(11) NOT NULL AUTO_INCREMENT,
                                                          `CPU` char(50) NOT NULL DEFAULT '',
                                                           PRIMARY KEY (`id`)
                                                           );";
    }
}