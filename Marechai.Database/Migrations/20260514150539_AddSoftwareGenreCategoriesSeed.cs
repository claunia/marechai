using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations;

/// <summary>
///     Seeds the <c>SoftwareGenres</c> table with 71 concrete entries of the
///     newly-introduced <c>SoftwareGenreType.Category</c> (= 4) value.
///
///     The category names are stored verbatim in English (no per-language
///     translation) and the source list's 11 grouping headers (Operating
///     Environments, System Infrastructure, Networking &amp; Internet,
///     Development &amp; Engineering, Productivity &amp; Office, Creative &amp;
///     Media Production, Media Consumption, Games &amp; Interactive Media,
///     Education, Science &amp; Research, Business, Finance &amp; Enterprise,
///     Consumer &amp; Lifestyle) are intentionally NOT seeded — only the 71
///     concrete leaf categories are.
///
///     Inserts are idempotent (gated by <c>WHERE NOT EXISTS</c> on the unique
///     <c>(Name, Type)</c> index) so re-running the migration on a partially-
///     seeded DB will not fail with a duplicate-key error.
/// </summary>
public partial class AddSoftwareGenreCategoriesSeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Operating Systems', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Operating Systems' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Firmware & Bootloaders', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Firmware & Bootloaders' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Virtual Machines & Hypervisors', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Virtual Machines & Hypervisors' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Runtime Environments', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Runtime Environments' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Device Drivers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Device Drivers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'System Libraries', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='System Libraries' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Kernel Extensions & Modules', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Kernel Extensions & Modules' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'System Services & Daemons', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='System Services & Daemons' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'System Utilities', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='System Utilities' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'File & Storage Utilities', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='File & Storage Utilities' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Backup & Archival Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Backup & Archival Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Compression & Packaging Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Compression & Packaging Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Diagnostics & Benchmarking', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Diagnostics & Benchmarking' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'System Configuration Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='System Configuration Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Security Utilities', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Security Utilities' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Network Clients', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Network Clients' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Network Servers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Network Servers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Network Utilities & Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Network Utilities & Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Communication Protocol Software', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Communication Protocol Software' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Cloud & Sync Clients', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Cloud & Sync Clients' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Internet Browsers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Internet Browsers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Programming Languages & Compilers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Programming Languages & Compilers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Assemblers & Linkers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Assemblers & Linkers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'IDEs & Code Editors', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='IDEs & Code Editors' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Debuggers & Profilers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Debuggers & Profilers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Build Systems & Package Managers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Build Systems & Package Managers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Version Control Systems', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Version Control Systems' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Database Engines', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Database Engines' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'API/SDK Tooling', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='API/SDK Tooling' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Engineering & Simulation Software', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Engineering & Simulation Software' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Office Suites', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Office Suites' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Document Editors', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Document Editors' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Spreadsheets', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Spreadsheets' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Presentation Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Presentation Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Note-Taking & Knowledge Management', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Note-Taking & Knowledge Management' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Project & Task Management', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Project & Task Management' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Communication & Collaboration Clients', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Communication & Collaboration Clients' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Graphics & Illustration', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Graphics & Illustration' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT '3D Modeling & CAD', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='3D Modeling & CAD' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Photography & Imaging', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Photography & Imaging' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Audio Production', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Audio Production' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Video Editing & Compositing', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Video Editing & Compositing' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Animation & Motion Graphics', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Animation & Motion Graphics' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Publishing & Layout Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Publishing & Layout Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Audio Players', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Audio Players' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Video Players', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Video Players' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Image Viewers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Image Viewers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Media Library Managers', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Media Library Managers' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Streaming Clients', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Streaming Clients' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Video Games', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Video Games' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Game Engines', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Game Engines' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Game Development Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Game Development Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Emulators', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Emulators' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Educational Software', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Educational Software' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Scientific Computing', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Scientific Computing' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Mathematical Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Mathematical Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Statistical Analysis', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Statistical Analysis' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Medical & Laboratory Software', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Medical & Laboratory Software' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Research Tools', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Research Tools' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'ERP Systems', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='ERP Systems' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'CRM Platforms', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='CRM Platforms' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Accounting & Finance Software', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Accounting & Finance Software' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Inventory & Logistics', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Inventory & Logistics' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Business Intelligence & Analytics', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Business Intelligence & Analytics' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Point-of-Sale Systems', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Point-of-Sale Systems' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Mapping & Navigation', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Mapping & Navigation' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Home Automation', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Home Automation' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Health & Fitness', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Health & Fitness' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Travel & Transportation', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Travel & Transportation' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Personal Finance', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Personal Finance' AND `Type`=4);
INSERT INTO `SoftwareGenres` (`Name`, `Type`, `CreatedOn`, `UpdatedOn`) SELECT 'Miscellaneous Consumer Apps', 4, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6) WHERE NOT EXISTS (SELECT 1 FROM `SoftwareGenres` WHERE `Name`='Miscellaneous Consumer Apps' AND `Type`=4);
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
        DELETE FROM `SoftwareGenres` WHERE `Type` = 4 AND `Name` IN (
            'Operating Systems',
            'Firmware & Bootloaders',
            'Virtual Machines & Hypervisors',
            'Runtime Environments',
            'Device Drivers',
            'System Libraries',
            'Kernel Extensions & Modules',
            'System Services & Daemons',
            'System Utilities',
            'File & Storage Utilities',
            'Backup & Archival Tools',
            'Compression & Packaging Tools',
            'Diagnostics & Benchmarking',
            'System Configuration Tools',
            'Security Utilities',
            'Network Clients',
            'Network Servers',
            'Network Utilities & Tools',
            'Communication Protocol Software',
            'Cloud & Sync Clients',
            'Internet Browsers',
            'Programming Languages & Compilers',
            'Assemblers & Linkers',
            'IDEs & Code Editors',
            'Debuggers & Profilers',
            'Build Systems & Package Managers',
            'Version Control Systems',
            'Database Engines',
            'API/SDK Tooling',
            'Engineering & Simulation Software',
            'Office Suites',
            'Document Editors',
            'Spreadsheets',
            'Presentation Tools',
            'Note-Taking & Knowledge Management',
            'Project & Task Management',
            'Communication & Collaboration Clients',
            'Graphics & Illustration',
            '3D Modeling & CAD',
            'Photography & Imaging',
            'Audio Production',
            'Video Editing & Compositing',
            'Animation & Motion Graphics',
            'Publishing & Layout Tools',
            'Audio Players',
            'Video Players',
            'Image Viewers',
            'Media Library Managers',
            'Streaming Clients',
            'Video Games',
            'Game Engines',
            'Game Development Tools',
            'Emulators',
            'Educational Software',
            'Scientific Computing',
            'Mathematical Tools',
            'Statistical Analysis',
            'Medical & Laboratory Software',
            'Research Tools',
            'ERP Systems',
            'CRM Platforms',
            'Accounting & Finance Software',
            'Inventory & Logistics',
            'Business Intelligence & Analytics',
            'Point-of-Sale Systems',
            'Mapping & Navigation',
            'Home Automation',
            'Health & Fitness',
            'Travel & Transportation',
            'Personal Finance',
            'Miscellaneous Consumer Apps'
        );
");
    }
}
