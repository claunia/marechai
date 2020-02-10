using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class Licenses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Licenses", table => new
            {
                Id = table.Column<int>().
                           Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name        = table.Column<string>(), SPDX      = table.Column<string>(nullable: true),
                FsfApproved = table.Column<bool>(), OsiApproved = table.Column<bool>(),
                Link        = table.Column<string>(maxLength: 512, nullable: true),
                Text        = table.Column<string>("longtext", maxLength: 131072, nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Licenses", x => x.Id);
            });

            migrationBuilder.InsertData("Licenses", new[]
            {
                "Id", "FsfApproved", "Link", "Name", "OsiApproved", "SPDX", "Text"
            }, new object[,]
            {
                {
                    1, false, null, "Fair use", false, null, null
                },
                {
                    247, false, "https://spdx.org/licenses/NPOSL-3.0.html#licenseText",
                    "Non-Profit Open Software License 3.0", true, "NPOSL-3.0", null
                },
                {
                    246, true, "https://spdx.org/licenses/NPL-1.1.html#licenseText", "Netscape Public License v1.1",
                    false, "NPL-1.1", null
                },
                {
                    245, true, "https://spdx.org/licenses/NPL-1.0.html#licenseText", "Netscape Public License v1.0",
                    false, "NPL-1.0", null
                },
                {
                    244, false, "https://spdx.org/licenses/Noweb.html#licenseText", "Noweb License", false, "Noweb",
                    null
                },
                {
                    243, true, "https://spdx.org/licenses/NOSL.html#licenseText", "Netizen Open Source License", false,
                    "NOSL", null
                },
                {
                    242, true, "https://spdx.org/licenses/Nokia.html#licenseText", "Nokia Open Source License", true,
                    "Nokia", null
                },
                {
                    241, false, "https://spdx.org/licenses/NLPL.html#licenseText", "No Limit Public License", false,
                    "NLPL", null
                },
                {
                    240, false, "https://spdx.org/licenses/NLOD-1.0.html#licenseText",
                    "Norwegian Licence for Open Government Data", false, "NLOD-1.0", null
                },
                {
                    248, false, "https://spdx.org/licenses/NRL.html#licenseText", "NRL License", false, "NRL", null
                },
                {
                    239, false, "https://spdx.org/licenses/NGPL.html#licenseText", "Nethack General Public License",
                    true, "NGPL", null
                },
                {
                    237, false, "https://spdx.org/licenses/NetCDF.html#licenseText", "NetCDF license", false, "NetCDF",
                    null
                },
                {
                    236, false, "https://spdx.org/licenses/Net-SNMP.html#licenseText", "Net-SNMP License", false,
                    "Net-SNMP", null
                },
                {
                    235, true, "https://spdx.org/licenses/NCSA.html#licenseText",
                    "University of Illinois/NCSA Open Source License", true, "NCSA", null
                },
                {
                    234, false, "https://spdx.org/licenses/NBPL-1.0.html#licenseText", "Net Boolean Public License v1",
                    false, "NBPL-1.0", null
                },
                {
                    233, false, "https://spdx.org/licenses/Naumen.html#licenseText", "Naumen Public License", true,
                    "Naumen", null
                },
                {
                    232, false, "https://spdx.org/licenses/NASA-1.3.html#licenseText", "NASA Open Source Agreement 1.3",
                    true, "NASA-1.3", null
                },
                {
                    231, false, "https://spdx.org/licenses/Mup.html#licenseText", "Mup License", false, "Mup", null
                },
                {
                    230, false, "https://spdx.org/licenses/Multics.html#licenseText", "Multics License", true,
                    "Multics", null
                },
                {
                    238, false, "https://spdx.org/licenses/Newsletr.html#licenseText", "Newsletr License", false,
                    "Newsletr", null
                },
                {
                    249, false, "https://spdx.org/licenses/NTP.html#licenseText", "NTP License", true, "NTP", null
                },
                {
                    250, false, "https://spdx.org/licenses/OCCT-PL.html#licenseText",
                    "Open CASCADE Technology Public License", false, "OCCT-PL", null
                },
                {
                    251, false, "https://spdx.org/licenses/OCLC-2.0.html#licenseText",
                    "OCLC Research Public License 2.0", true, "OCLC-2.0", null
                },
                {
                    270, true, "https://spdx.org/licenses/OLDAP-2.3.html#licenseText", "Open LDAP Public License v2.3",
                    false, "OLDAP-2.3", null
                },
                {
                    269, false, "https://spdx.org/licenses/OLDAP-2.2.2.html#licenseText",
                    "Open LDAP Public License 2.2.2", false, "OLDAP-2.2.2", null
                },
                {
                    268, false, "https://spdx.org/licenses/OLDAP-2.2.1.html#licenseText",
                    "Open LDAP Public License v2.2.1", false, "OLDAP-2.2.1", null
                },
                {
                    267, false, "https://spdx.org/licenses/OLDAP-2.2.html#licenseText", "Open LDAP Public License v2.2",
                    false, "OLDAP-2.2", null
                },
                {
                    266, false, "https://spdx.org/licenses/OLDAP-2.1.html#licenseText", "Open LDAP Public License v2.1",
                    false, "OLDAP-2.1", null
                },
                {
                    265, false, "https://spdx.org/licenses/OLDAP-2.0.1.html#licenseText",
                    "Open LDAP Public License v2.0.1", false, "OLDAP-2.0.1", null
                },
                {
                    264, false, "https://spdx.org/licenses/OLDAP-2.0.html#licenseText",
                    "Open LDAP Public License v2.0 (or possibly 2.0A and 2.0B)", false, "OLDAP-2.0", null
                },
                {
                    263, false, "https://spdx.org/licenses/OLDAP-1.4.html#licenseText", "Open LDAP Public License v1.4",
                    false, "OLDAP-1.4", null
                },
                {
                    262, false, "https://spdx.org/licenses/OLDAP-1.3.html#licenseText", "Open LDAP Public License v1.3",
                    false, "OLDAP-1.3", null
                },
                {
                    261, false, "https://spdx.org/licenses/OLDAP-1.2.html#licenseText", "Open LDAP Public License v1.2",
                    false, "OLDAP-1.2", null
                },
                {
                    260, false, "https://spdx.org/licenses/OLDAP-1.1.html#licenseText", "Open LDAP Public License v1.1",
                    false, "OLDAP-1.1", null
                },
                {
                    259, false, "https://spdx.org/licenses/OGTSL.html#licenseText", "Open Group Test Suite License",
                    true, "OGTSL", null
                },
                {
                    258, false, "https://spdx.org/licenses/OGL-UK-3.0.html#licenseText", "Open Government Licence v3.0",
                    false, "OGL-UK-3.0", null
                },
                {
                    257, false, "https://spdx.org/licenses/OGL-UK-2.0.html#licenseText", "Open Government Licence v2.0",
                    false, "OGL-UK-2.0", null
                },
                {
                    256, false, "https://spdx.org/licenses/OGL-UK-1.0.html#licenseText", "Open Government Licence v1.0",
                    false, "OGL-UK-1.0", null
                },
                {
                    255, true, "https://spdx.org/licenses/OFL-1.1.html#licenseText", "SIL Open Font License 1.1", true,
                    "OFL-1.1", null
                },
                {
                    254, true, "https://spdx.org/licenses/OFL-1.0.html#licenseText", "SIL Open Font License 1.0", false,
                    "OFL-1.0", null
                },
                {
                    253, false, "https://spdx.org/licenses/ODC-By-1.0.html#licenseText",
                    "Open Data Commons Attribution License v1.0", false, "ODC-By-1.0", null
                },
                {
                    252, true, "https://spdx.org/licenses/ODbL-1.0.html#licenseText", "ODC Open Database License v1.0",
                    false, "ODbL-1.0", null
                },
                {
                    229, false, "https://spdx.org/licenses/MTLL.html#licenseText", "Matrix Template Library License",
                    false, "MTLL", null
                },
                {
                    228, true, "https://spdx.org/licenses/MS-RL.html#licenseText", "Microsoft Reciprocal License", true,
                    "MS-RL", null
                },
                {
                    227, true, "https://spdx.org/licenses/MS-PL.html#licenseText", "Microsoft Public License", true,
                    "MS-PL", null
                },
                {
                    226, false, "https://spdx.org/licenses/MPL-2.0-no-copyleft-exception.html#licenseText",
                    "Mozilla Public License 2.0 (no copyleft exception)", true, "MPL-2.0-no-copyleft-exception", null
                },
                {
                    202, false, "https://spdx.org/licenses/LiLiQ-R-1.1.html#licenseText",
                    "Licence Libre du Québec – Réciprocité version 1.1", true, "LiLiQ-R-1.1", null
                },
                {
                    201, false, "https://spdx.org/licenses/LiLiQ-P-1.1.html#licenseText",
                    "Licence Libre du Québec – Permissive version 1.1", true, "LiLiQ-P-1.1", null
                },
                {
                    200, false, "https://spdx.org/licenses/libtiff.html#licenseText", "libtiff License", false,
                    "libtiff", null
                },
                {
                    199, false, "https://spdx.org/licenses/libpng-2.0.html#licenseText",
                    "PNG Reference Library version 2", false, "libpng-2.0", null
                },
                {
                    198, false, "https://spdx.org/licenses/Libpng.html#licenseText", "libpng License", false, "Libpng",
                    null
                },
                {
                    197, false, "https://spdx.org/licenses/LGPLLR.html#licenseText",
                    "Lesser General Public License For Linguistic Resources", false, "LGPLLR", null
                },
                {
                    196, true, "https://spdx.org/licenses/LGPL-3.0-or-later.html#licenseText",
                    "GNU Lesser General Public License v3.0 or later", true, "LGPL-3.0-or-later", null
                },
                {
                    195, true, "https://spdx.org/licenses/LGPL-3.0-only.html#licenseText",
                    "GNU Lesser General Public License v3.0 only", true, "LGPL-3.0-only", null
                },
                {
                    194, true, "https://spdx.org/licenses/LGPL-2.1-or-later.html#licenseText",
                    "GNU Lesser General Public License v2.1 or later", true, "LGPL-2.1-or-later", null
                },
                {
                    193, true, "https://spdx.org/licenses/LGPL-2.1-only.html#licenseText",
                    "GNU Lesser General Public License v2.1 only", true, "LGPL-2.1-only", null
                },
                {
                    192, false, "https://spdx.org/licenses/LGPL-2.0-or-later.html#licenseText",
                    "GNU Library General Public License v2 or later", true, "LGPL-2.0-or-later", null
                },
                {
                    191, false, "https://spdx.org/licenses/LGPL-2.0-only.html#licenseText",
                    "GNU Library General Public License v2 only", true, "LGPL-2.0-only", null
                },
                {
                    190, false, "https://spdx.org/licenses/Leptonica.html#licenseText", "Leptonica License", false,
                    "Leptonica", null
                },
                {
                    189, false, "https://spdx.org/licenses/Latex2e.html#licenseText", "Latex2e License", false,
                    "Latex2e", null
                },
                {
                    188, false, "https://spdx.org/licenses/LAL-1.3.html#licenseText", "Licence Art Libre 1.3", false,
                    "LAL-1.3", null
                },
                {
                    187, false, "https://spdx.org/licenses/LAL-1.2.html#licenseText", "Licence Art Libre 1.2", false,
                    "LAL-1.2", null
                },
                {
                    186, false, "https://spdx.org/licenses/JSON.html#licenseText", "JSON License", false, "JSON", null
                },
                {
                    185, false, "https://spdx.org/licenses/JPNIC.html#licenseText",
                    "Japan Network Information Center License", false, "JPNIC", null
                },
                {
                    184, false, "https://spdx.org/licenses/JasPer-2.0.html#licenseText", "JasPer License", false,
                    "JasPer-2.0", null
                },
                {
                    203, false, "https://spdx.org/licenses/LiLiQ-Rplus-1.1.html#licenseText",
                    "Licence Libre du Québec – Réciprocité forte version 1.1", true, "LiLiQ-Rplus-1.1", null
                },
                {
                    271, false, "https://spdx.org/licenses/OLDAP-2.4.html#licenseText", "Open LDAP Public License v2.4",
                    false, "OLDAP-2.4", null
                },
                {
                    204, false, "https://spdx.org/licenses/Linux-OpenIB.html#licenseText",
                    "Linux Kernel Variant of OpenIB.org license", false, "Linux-OpenIB", null
                },
                {
                    206, true, "https://spdx.org/licenses/LPL-1.02.html#licenseText", "Lucent Public License v1.02",
                    true, "LPL-1.02", null
                },
                {
                    225, true, "https://spdx.org/licenses/MPL-2.0.html#licenseText", "Mozilla Public License 2.0", true,
                    "MPL-2.0", null
                },
                {
                    224, true, "https://spdx.org/licenses/MPL-1.1.html#licenseText", "Mozilla Public License 1.1", true,
                    "MPL-1.1", null
                },
                {
                    223, false, "https://spdx.org/licenses/MPL-1.0.html#licenseText", "Mozilla Public License 1.0",
                    true, "MPL-1.0", null
                },
                {
                    222, false, "https://spdx.org/licenses/mpich2.html#licenseText", "mpich2 License", false, "mpich2",
                    null
                },
                {
                    221, false, "https://spdx.org/licenses/Motosoto.html#licenseText", "Motosoto License", true,
                    "Motosoto", null
                },
                {
                    220, false, "https://spdx.org/licenses/MITNFA.html#licenseText", "MIT +no-false-attribs license",
                    false, "MITNFA", null
                },
                {
                    219, false, "https://spdx.org/licenses/MIT-feh.html#licenseText", "feh License", false, "MIT-feh",
                    null
                },
                {
                    218, false, "https://spdx.org/licenses/MIT-enna.html#licenseText", "enna License", false,
                    "MIT-enna", null
                },
                {
                    217, false, "https://spdx.org/licenses/MIT-CMU.html#licenseText", "CMU License", false, "MIT-CMU",
                    null
                },
                {
                    216, false, "https://spdx.org/licenses/MIT-advertising.html#licenseText",
                    "Enlightenment License (e16)", false, "MIT-advertising", null
                },
                {
                    215, false, "https://spdx.org/licenses/MIT-0.html#licenseText", "MIT No Attribution", true, "MIT-0",
                    null
                },
                {
                    214, true, "https://spdx.org/licenses/MIT.html#licenseText", "MIT License", true, "MIT", null
                },
                {
                    213, false, "https://spdx.org/licenses/MirOS.html#licenseText", "MirOS License", true, "MirOS", null
                },
                {
                    212, false, "https://spdx.org/licenses/MakeIndex.html#licenseText", "MakeIndex License", false,
                    "MakeIndex", null
                },
                {
                    211, false, "https://spdx.org/licenses/LPPL-1.3c.html#licenseText",
                    "LaTeX Project Public License v1.3c", true, "LPPL-1.3c", null
                },
                {
                    210, true, "https://spdx.org/licenses/LPPL-1.3a.html#licenseText",
                    "LaTeX Project Public License v1.3a", false, "LPPL-1.3a", null
                },
                {
                    209, true, "https://spdx.org/licenses/LPPL-1.2.html#licenseText",
                    "LaTeX Project Public License v1.2", false, "LPPL-1.2", null
                },
                {
                    208, false, "https://spdx.org/licenses/LPPL-1.1.html#licenseText",
                    "LaTeX Project Public License v1.1", false, "LPPL-1.1", null
                },
                {
                    207, false, "https://spdx.org/licenses/LPPL-1.0.html#licenseText",
                    "LaTeX Project Public License v1.0", false, "LPPL-1.0", null
                },
                {
                    205, false, "https://spdx.org/licenses/LPL-1.0.html#licenseText",
                    "Lucent Public License Version 1.0", true, "LPL-1.0", null
                },
                {
                    183, true, "https://spdx.org/licenses/ISC.html#licenseText", "ISC License", true, "ISC", null
                },
                {
                    272, false, "https://spdx.org/licenses/OLDAP-2.5.html#licenseText", "Open LDAP Public License v2.5",
                    false, "OLDAP-2.5", null
                },
                {
                    274, true, "https://spdx.org/licenses/OLDAP-2.7.html#licenseText", "Open LDAP Public License v2.7",
                    false, "OLDAP-2.7", null
                },
                {
                    338, false, "https://spdx.org/licenses/VOSTROM.html#licenseText",
                    "VOSTROM Public License for Open Source", false, "VOSTROM", null
                },
                {
                    337, true, "https://spdx.org/licenses/Vim.html#licenseText", "Vim License", false, "Vim", null
                },
                {
                    336, true, "https://spdx.org/licenses/UPL-1.0.html#licenseText",
                    "Universal Permissive License v1.0", true, "UPL-1.0", null
                },
                {
                    335, true, "https://spdx.org/licenses/Unlicense.html#licenseText", "The Unlicense", false,
                    "Unlicense", null
                },
                {
                    334, false, "https://spdx.org/licenses/Unicode-TOU.html#licenseText", "Unicode Terms of Use", false,
                    "Unicode-TOU", null
                },
                {
                    333, false, "https://spdx.org/licenses/Unicode-DFS-2016.html#licenseText",
                    "Unicode License Agreement - Data Files and Software (2016)", false, "Unicode-DFS-2016", null
                },
                {
                    332, false, "https://spdx.org/licenses/Unicode-DFS-2015.html#licenseText",
                    "Unicode License Agreement - Data Files and Software (2015)", false, "Unicode-DFS-2015", null
                },
                {
                    331, false, "https://spdx.org/licenses/TU-Berlin-2.0.html#licenseText",
                    "Technische Universitaet Berlin License 2.0", false, "TU-Berlin-2.0", null
                },
                {
                    339, false, "https://spdx.org/licenses/VSL-1.0.html#licenseText", "Vovida Software License v1.0",
                    true, "VSL-1.0", null
                },
                {
                    330, false, "https://spdx.org/licenses/TU-Berlin-1.0.html#licenseText",
                    "Technische Universitaet Berlin License 1.0", false, "TU-Berlin-1.0", null
                },
                {
                    328, false, "https://spdx.org/licenses/TORQUE-1.1.html#licenseText",
                    "TORQUE v2.5+ Software License v1.1", false, "TORQUE-1.1", null
                },
                {
                    327, false, "https://spdx.org/licenses/TMate.html#licenseText", "TMate Open Source License", false,
                    "TMate", null
                },
                {
                    326, false, "https://spdx.org/licenses/TCP-wrappers.html#licenseText", "TCP Wrappers License",
                    false, "TCP-wrappers", null
                },
                {
                    325, false, "https://spdx.org/licenses/TCL.html#licenseText", "TCL/TK License", false, "TCL", null
                },
                {
                    324, false, "https://spdx.org/licenses/TAPR-OHL-1.0.html#licenseText",
                    "TAPR Open Hardware License v1.0", false, "TAPR-OHL-1.0", null
                },
                {
                    323, false, "https://spdx.org/licenses/SWL.html#licenseText",
                    "Scheme Widget Library (SWL) Software License Agreement", false, "SWL", null
                },
                {
                    322, false, "https://spdx.org/licenses/SugarCRM-1.1.3.html#licenseText",
                    "SugarCRM Public License v1.1.3", false, "SugarCRM-1.1.3", null
                },
                {
                    321, true, "https://spdx.org/licenses/SPL-1.0.html#licenseText", "Sun Public License v1.0", true,
                    "SPL-1.0", null
                },
                {
                    329, false, "https://spdx.org/licenses/TOSL.html#licenseText", "Trusster Open Source License",
                    false, "TOSL", null
                },
                {
                    340, true, "https://spdx.org/licenses/W3C.html#licenseText",
                    "W3C Software Notice and License (2002-12-31)", true, "W3C", null
                },
                {
                    341, false, "https://spdx.org/licenses/W3C-19980720.html#licenseText",
                    "W3C Software Notice and License (1998-07-20)", false, "W3C-19980720", null
                },
                {
                    342, false, "https://spdx.org/licenses/W3C-20150513.html#licenseText",
                    "W3C Software Notice and Document License (2015-05-13)", false, "W3C-20150513", null
                },
                {
                    361, false, "https://spdx.org/licenses/ZPL-1.1.html#licenseText", "Zope Public License 1.1", false,
                    "ZPL-1.1", null
                },
                {
                    360, false, "https://spdx.org/licenses/zlib-acknowledgement.html#licenseText",
                    "zlib/libpng License with Acknowledgement", false, "zlib-acknowledgement", null
                },
                {
                    359, true, "https://spdx.org/licenses/Zlib.html#licenseText", "zlib License", true, "Zlib", null
                },
                {
                    358, false, "https://spdx.org/licenses/Zimbra-1.4.html#licenseText", "Zimbra Public License v1.4",
                    false, "Zimbra-1.4", null
                },
                {
                    357, true, "https://spdx.org/licenses/Zimbra-1.3.html#licenseText", "Zimbra Public License v1.3",
                    false, "Zimbra-1.3", null
                },
                {
                    356, true, "https://spdx.org/licenses/Zend-2.0.html#licenseText", "Zend License v2.0", false,
                    "Zend-2.0", null
                },
                {
                    355, false, "https://spdx.org/licenses/Zed.html#licenseText", "Zed License", false, "Zed", null
                },
                {
                    354, true, "https://spdx.org/licenses/YPL-1.1.html#licenseText", "Yahoo! Public License v1.1",
                    false, "YPL-1.1", null
                },
                {
                    353, false, "https://spdx.org/licenses/YPL-1.0.html#licenseText", "Yahoo! Public License v1.0",
                    false, "YPL-1.0", null
                },
                {
                    352, false, "https://spdx.org/licenses/XSkat.html#licenseText", "XSkat License", false, "XSkat",
                    null
                },
                {
                    351, false, "https://spdx.org/licenses/xpp.html#licenseText", "XPP License", false, "xpp", null
                },
                {
                    350, false, "https://spdx.org/licenses/Xnet.html#licenseText", "X.Net License", true, "Xnet", null
                },
                {
                    349, true, "https://spdx.org/licenses/xinetd.html#licenseText", "xinetd License", false, "xinetd",
                    null
                },
                {
                    348, true, "https://spdx.org/licenses/XFree86-1.1.html#licenseText", "XFree86 License 1.1", false,
                    "XFree86-1.1", null
                },
                {
                    347, false, "https://spdx.org/licenses/Xerox.html#licenseText", "Xerox License", false, "Xerox",
                    null
                },
                {
                    346, true, "https://spdx.org/licenses/X11.html#licenseText", "X11 License", false, "X11", null
                },
                {
                    345, true, "https://spdx.org/licenses/WTFPL.html#licenseText",
                    "Do What The F*ck You Want To Public License", false, "WTFPL", null
                },
                {
                    344, false, "https://spdx.org/licenses/Wsuipa.html#licenseText", "Wsuipa License", false, "Wsuipa",
                    null
                },
                {
                    343, false, "https://spdx.org/licenses/Watcom-1.0.html#licenseText",
                    "Sybase Open Watcom Public License 1.0", true, "Watcom-1.0", null
                },
                {
                    320, false, "https://spdx.org/licenses/Spencer-99.html#licenseText", "Spencer License 99", false,
                    "Spencer-99", null
                },
                {
                    319, false, "https://spdx.org/licenses/Spencer-94.html#licenseText", "Spencer License 94", false,
                    "Spencer-94", null
                },
                {
                    318, false, "https://spdx.org/licenses/Spencer-86.html#licenseText", "Spencer License 86", false,
                    "Spencer-86", null
                },
                {
                    317, false, "https://spdx.org/licenses/SNIA.html#licenseText", "SNIA Public License 1.1", false,
                    "SNIA", null
                },
                {
                    293, false, "https://spdx.org/licenses/Qhull.html#licenseText", "Qhull License", false, "Qhull",
                    null
                },
                {
                    292, true, "https://spdx.org/licenses/Python-2.0.html#licenseText", "Python License 2.0", true,
                    "Python-2.0", null
                },
                {
                    291, false, "https://spdx.org/licenses/psutils.html#licenseText", "psutils License", false,
                    "psutils", null
                },
                {
                    290, false, "https://spdx.org/licenses/psfrag.html#licenseText", "psfrag License", false, "psfrag",
                    null
                },
                {
                    289, false, "https://spdx.org/licenses/PostgreSQL.html#licenseText", "PostgreSQL License", true,
                    "PostgreSQL", null
                },
                {
                    288, false, "https://spdx.org/licenses/Plexus.html#licenseText", "Plexus Classworlds License",
                    false, "Plexus", null
                },
                {
                    287, true, "https://spdx.org/licenses/PHP-3.01.html#licenseText", "PHP License v3.01", false,
                    "PHP-3.01", null
                },
                {
                    286, false, "https://spdx.org/licenses/PHP-3.0.html#licenseText", "PHP License v3.0", true,
                    "PHP-3.0", null
                },
                {
                    285, false, "https://spdx.org/licenses/PDDL-1.0.html#licenseText",
                    "ODC Public Domain Dedication & License 1.0", false, "PDDL-1.0", null
                },
                {
                    284, true, "https://spdx.org/licenses/OSL-3.0.html#licenseText", "Open Software License 3.0", true,
                    "OSL-3.0", null
                },
                {
                    283, true, "https://spdx.org/licenses/OSL-2.1.html#licenseText", "Open Software License 2.1", true,
                    "OSL-2.1", null
                },
                {
                    282, true, "https://spdx.org/licenses/OSL-2.0.html#licenseText", "Open Software License 2.0", true,
                    "OSL-2.0", null
                },
                {
                    281, true, "https://spdx.org/licenses/OSL-1.1.html#licenseText", "Open Software License 1.1", false,
                    "OSL-1.1", null
                },
                {
                    280, true, "https://spdx.org/licenses/OSL-1.0.html#licenseText", "Open Software License 1.0", true,
                    "OSL-1.0", null
                },
                {
                    279, false, "https://spdx.org/licenses/OSET-PL-2.1.html#licenseText",
                    "OSET Public License version 2.1", true, "OSET-PL-2.1", null
                },
                {
                    278, false, "https://spdx.org/licenses/OPL-1.0.html#licenseText", "Open Public License v1.0", false,
                    "OPL-1.0", null
                },
                {
                    277, true, "https://spdx.org/licenses/OpenSSL.html#licenseText", "OpenSSL License", false,
                    "OpenSSL", null
                },
                {
                    276, false, "https://spdx.org/licenses/OML.html#licenseText", "Open Market License", false, "OML",
                    null
                },
                {
                    275, false, "https://spdx.org/licenses/OLDAP-2.8.html#licenseText", "Open LDAP Public License v2.8",
                    false, "OLDAP-2.8", null
                },
                {
                    294, true, "https://spdx.org/licenses/QPL-1.0.html#licenseText", "Q Public License 1.0", true,
                    "QPL-1.0", null
                },
                {
                    273, false, "https://spdx.org/licenses/OLDAP-2.6.html#licenseText", "Open LDAP Public License v2.6",
                    false, "OLDAP-2.6", null
                },
                {
                    295, false, "https://spdx.org/licenses/Rdisc.html#licenseText", "Rdisc License", false, "Rdisc",
                    null
                },
                {
                    297, false, "https://spdx.org/licenses/RPL-1.1.html#licenseText", "Reciprocal Public License 1.1",
                    true, "RPL-1.1", null
                },
                {
                    316, false, "https://spdx.org/licenses/SMPPL.html#licenseText",
                    "Secure Messaging Protocol Public License", false, "SMPPL", null
                },
                {
                    315, true, "https://spdx.org/licenses/SMLNJ.html#licenseText", "Standard ML of New Jersey License",
                    false, "SMLNJ", null
                },
                {
                    314, true, "https://spdx.org/licenses/Sleepycat.html#licenseText", "Sleepycat License", true,
                    "Sleepycat", null
                },
                {
                    313, false, "https://spdx.org/licenses/SISSL-1.2.html#licenseText",
                    "Sun Industry Standards Source License v1.2", false, "SISSL-1.2", null
                },
                {
                    312, true, "https://spdx.org/licenses/SISSL.html#licenseText",
                    "Sun Industry Standards Source License v1.1", true, "SISSL", null
                },
                {
                    311, false, "https://spdx.org/licenses/SimPL-2.0.html#licenseText", "Simple Public License 2.0",
                    true, "SimPL-2.0", null
                },
                {
                    310, true, "https://spdx.org/licenses/SGI-B-2.0.html#licenseText",
                    "SGI Free Software License B v2.0", false, "SGI-B-2.0", null
                },
                {
                    309, false, "https://spdx.org/licenses/SGI-B-1.1.html#licenseText",
                    "SGI Free Software License B v1.1", false, "SGI-B-1.1", null
                },
                {
                    308, false, "https://spdx.org/licenses/SGI-B-1.0.html#licenseText",
                    "SGI Free Software License B v1.0", false, "SGI-B-1.0", null
                },
                {
                    307, false, "https://spdx.org/licenses/Sendmail-8.23.html#licenseText", "Sendmail License 8.23",
                    false, "Sendmail-8.23", null
                },
                {
                    306, false, "https://spdx.org/licenses/Sendmail.html#licenseText", "Sendmail License", false,
                    "Sendmail", null
                },
                {
                    305, false, "https://spdx.org/licenses/SCEA.html#licenseText", "SCEA Shared Source License", false,
                    "SCEA", null
                },
                {
                    304, false, "https://spdx.org/licenses/Saxpath.html#licenseText", "Saxpath License", false,
                    "Saxpath", null
                },
                {
                    303, false, "https://spdx.org/licenses/SAX-PD.html#licenseText", "Sax Public Domain Notice", false,
                    "SAX-PD", null
                },
                {
                    302, true, "https://spdx.org/licenses/Ruby.html#licenseText", "Ruby License", false, "Ruby", null
                },
                {
                    301, false, "https://spdx.org/licenses/RSCPL.html#licenseText", "Ricoh Source Code Public License",
                    true, "RSCPL", null
                },
                {
                    300, false, "https://spdx.org/licenses/RSA-MD.html#licenseText", "RSA Message-Digest License",
                    false, "RSA-MD", null
                },
                {
                    299, true, "https://spdx.org/licenses/RPSL-1.0.html#licenseText",
                    "RealNetworks Public Source License v1.0", true, "RPSL-1.0", null
                },
                {
                    298, false, "https://spdx.org/licenses/RPL-1.5.html#licenseText", "Reciprocal Public License 1.5",
                    true, "RPL-1.5", null
                },
                {
                    296, false, "https://spdx.org/licenses/RHeCos-1.1.html#licenseText",
                    "Red Hat eCos Public License v1.1", false, "RHeCos-1.1", null
                },
                {
                    362, true, "https://spdx.org/licenses/ZPL-2.0.html#licenseText", "Zope Public License 2.0", true,
                    "ZPL-2.0", null
                },
                {
                    182, true, "https://spdx.org/licenses/IPL-1.0.html#licenseText", "IBM Public License v1.0", true,
                    "IPL-1.0", null
                },
                {
                    180, false, "https://spdx.org/licenses/Interbase-1.0.html#licenseText",
                    "Interbase Public License v1.0", false, "Interbase-1.0", null
                },
                {
                    65, false, "https://spdx.org/licenses/CC-BY-1.0.html#licenseText",
                    "Creative Commons Attribution 1.0 Generic", false, "CC-BY-1.0", null
                },
                {
                    64, false, "https://spdx.org/licenses/CATOSL-1.1.html#licenseText",
                    "Computer Associates Trusted Open Source License 1.1", true, "CATOSL-1.1", null
                },
                {
                    63, false, "https://spdx.org/licenses/Caldera.html#licenseText", "Caldera License", false,
                    "Caldera", null
                },
                {
                    62, false, "https://spdx.org/licenses/bzip2-1.0.6.html#licenseText",
                    "bzip2 and libbzip2 License v1.0.6", false, "bzip2-1.0.6", null
                },
                {
                    61, false, "https://spdx.org/licenses/bzip2-1.0.5.html#licenseText",
                    "bzip2 and libbzip2 License v1.0.5", false, "bzip2-1.0.5", null
                },
                {
                    60, true, "https://spdx.org/licenses/BSL-1.0.html#licenseText", "Boost Software License 1.0", true,
                    "BSL-1.0", null
                },
                {
                    59, false, "https://spdx.org/licenses/BSD-Source-Code.html#licenseText",
                    "BSD Source Code Attribution", false, "BSD-Source-Code", null
                },
                {
                    58, false, "https://spdx.org/licenses/BSD-Protection.html#licenseText", "BSD Protection License",
                    false, "BSD-Protection", null
                },
                {
                    66, false, "https://spdx.org/licenses/CC-BY-2.0.html#licenseText",
                    "Creative Commons Attribution 2.0 Generic", false, "CC-BY-2.0", null
                },
                {
                    57, false, "https://spdx.org/licenses/BSD-4-Clause-UC.html#licenseText",
                    "BSD-4-Clause (University of California-Specific)", false, "BSD-4-Clause-UC", null
                },
                {
                    55, false, "https://spdx.org/licenses/BSD-3-Clause-No-Nuclear-Warranty.html#licenseText",
                    "BSD 3-Clause No Nuclear Warranty", false, "BSD-3-Clause-No-Nuclear-Warranty", null
                },
                {
                    54, false, "https://spdx.org/licenses/BSD-3-Clause-No-Nuclear-License-2014.html#licenseText",
                    "BSD 3-Clause No Nuclear License 2014", false, "BSD-3-Clause-No-Nuclear-License-2014", null
                },
                {
                    53, false, "https://spdx.org/licenses/BSD-3-Clause-No-Nuclear-License.html#licenseText",
                    "BSD 3-Clause No Nuclear License", false, "BSD-3-Clause-No-Nuclear-License", null
                },
                {
                    52, false, "https://spdx.org/licenses/BSD-3-Clause-LBNL.html#licenseText",
                    "Lawrence Berkeley National Labs BSD variant license", false, "BSD-3-Clause-LBNL", null
                },
                {
                    51, true, "https://spdx.org/licenses/BSD-3-Clause-Clear.html#licenseText",
                    "BSD 3-Clause Clear License", false, "BSD-3-Clause-Clear", null
                },
                {
                    50, false, "https://spdx.org/licenses/BSD-3-Clause-Attribution.html#licenseText",
                    "BSD with attribution", false, "BSD-3-Clause-Attribution", null
                },
                {
                    49, true, "https://spdx.org/licenses/BSD-3-Clause.html#licenseText",
                    "BSD 3-Clause \"New\" or \"Revised\" License", true, "BSD-3-Clause", null
                },
                {
                    48, false, "https://spdx.org/licenses/BSD-2-Clause-Patent.html#licenseText",
                    "BSD-2-Clause Plus Patent License", true, "BSD-2-Clause-Patent", null
                },
                {
                    56, true, "https://spdx.org/licenses/BSD-4-Clause.html#licenseText",
                    "BSD 4-Clause \"Original\" or \"Old\" License", false, "BSD-4-Clause", null
                },
                {
                    67, false, "https://spdx.org/licenses/CC-BY-2.5.html#licenseText",
                    "Creative Commons Attribution 2.5 Generic", false, "CC-BY-2.5", null
                },
                {
                    68, false, "https://spdx.org/licenses/CC-BY-3.0.html#licenseText",
                    "Creative Commons Attribution 3.0 Unported", false, "CC-BY-3.0", null
                },
                {
                    69, true, "https://spdx.org/licenses/CC-BY-4.0.html#licenseText",
                    "Creative Commons Attribution 4.0 International", false, "CC-BY-4.0", null
                },
                {
                    88, false, "https://spdx.org/licenses/CC-BY-ND-3.0.html#licenseText",
                    "Creative Commons Attribution No Derivatives 3.0 Unported", false, "CC-BY-ND-3.0", null
                },
                {
                    87, false, "https://spdx.org/licenses/CC-BY-ND-2.5.html#licenseText",
                    "Creative Commons Attribution No Derivatives 2.5 Generic", false, "CC-BY-ND-2.5", null
                },
                {
                    86, false, "https://spdx.org/licenses/CC-BY-ND-2.0.html#licenseText",
                    "Creative Commons Attribution No Derivatives 2.0 Generic", false, "CC-BY-ND-2.0", null
                },
                {
                    85, false, "https://spdx.org/licenses/CC-BY-ND-1.0.html#licenseText",
                    "Creative Commons Attribution No Derivatives 1.0 Generic", false, "CC-BY-ND-1.0", null
                },
                {
                    84, false, "https://spdx.org/licenses/CC-BY-NC-SA-4.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial Share Alike 4.0 International", false,
                    "CC-BY-NC-SA-4.0", null
                },
                {
                    83, false, "https://spdx.org/licenses/CC-BY-NC-SA-3.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial Share Alike 3.0 Unported", false, "CC-BY-NC-SA-3.0",
                    null
                },
                {
                    82, false, "https://spdx.org/licenses/CC-BY-NC-SA-2.5.html#licenseText",
                    "Creative Commons Attribution Non Commercial Share Alike 2.5 Generic", false, "CC-BY-NC-SA-2.5",
                    null
                },
                {
                    81, false, "https://spdx.org/licenses/CC-BY-NC-SA-2.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial Share Alike 2.0 Generic", false, "CC-BY-NC-SA-2.0",
                    null
                },
                {
                    80, false, "https://spdx.org/licenses/CC-BY-NC-SA-1.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial Share Alike 1.0 Generic", false, "CC-BY-NC-SA-1.0",
                    null
                },
                {
                    79, false, "https://spdx.org/licenses/CC-BY-NC-ND-4.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial No Derivatives 4.0 International", false,
                    "CC-BY-NC-ND-4.0", null
                },
                {
                    78, false, "https://spdx.org/licenses/CC-BY-NC-ND-3.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial No Derivatives 3.0 Unported", false, "CC-BY-NC-ND-3.0",
                    null
                },
                {
                    77, false, "https://spdx.org/licenses/CC-BY-NC-ND-2.5.html#licenseText",
                    "Creative Commons Attribution Non Commercial No Derivatives 2.5 Generic", false, "CC-BY-NC-ND-2.5",
                    null
                },
                {
                    76, false, "https://spdx.org/licenses/CC-BY-NC-ND-2.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial No Derivatives 2.0 Generic", false, "CC-BY-NC-ND-2.0",
                    null
                },
                {
                    75, false, "https://spdx.org/licenses/CC-BY-NC-ND-1.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial No Derivatives 1.0 Generic", false, "CC-BY-NC-ND-1.0",
                    null
                },
                {
                    74, false, "https://spdx.org/licenses/CC-BY-NC-4.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial 4.0 International", false, "CC-BY-NC-4.0", null
                },
                {
                    73, false, "https://spdx.org/licenses/CC-BY-NC-3.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial 3.0 Unported", false, "CC-BY-NC-3.0", null
                },
                {
                    72, false, "https://spdx.org/licenses/CC-BY-NC-2.5.html#licenseText",
                    "Creative Commons Attribution Non Commercial 2.5 Generic", false, "CC-BY-NC-2.5", null
                },
                {
                    71, false, "https://spdx.org/licenses/CC-BY-NC-2.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial 2.0 Generic", false, "CC-BY-NC-2.0", null
                },
                {
                    70, false, "https://spdx.org/licenses/CC-BY-NC-1.0.html#licenseText",
                    "Creative Commons Attribution Non Commercial 1.0 Generic", false, "CC-BY-NC-1.0", null
                },
                {
                    47, false, "https://spdx.org/licenses/BSD-2-Clause-NetBSD.html#licenseText",
                    "BSD 2-Clause NetBSD License", false, "BSD-2-Clause-NetBSD", null
                },
                {
                    46, true, "https://spdx.org/licenses/BSD-2-Clause-FreeBSD.html#licenseText",
                    "BSD 2-Clause FreeBSD License", false, "BSD-2-Clause-FreeBSD", null
                },
                {
                    45, false, "https://spdx.org/licenses/BSD-2-Clause.html#licenseText",
                    "BSD 2-Clause \"Simplified\" License", true, "BSD-2-Clause", null
                },
                {
                    44, false, "https://spdx.org/licenses/BSD-1-Clause.html#licenseText", "BSD 1-Clause License", false,
                    "BSD-1-Clause", null
                },
                {
                    20, false, "https://spdx.org/licenses/Aladdin.html#licenseText", "Aladdin Free Public License",
                    false, "Aladdin", null
                },
                {
                    19, true, "https://spdx.org/licenses/AGPL-3.0-or-later.html#licenseText",
                    "GNU Affero General Public License v3.0 or later", true, "AGPL-3.0-or-later", null
                },
                {
                    18, true, "https://spdx.org/licenses/AGPL-3.0-only.html#licenseText",
                    "GNU Affero General Public License v3.0 only", true, "AGPL-3.0-only", null
                },
                {
                    17, false, "https://spdx.org/licenses/AGPL-1.0-or-later.html#licenseText",
                    "Affero General Public License v1.0 or later", false, "AGPL-1.0-or-later", null
                },
                {
                    16, false, "https://spdx.org/licenses/AGPL-1.0-only.html#licenseText",
                    "Affero General Public License v1.0 only", false, "AGPL-1.0-only", null
                },
                {
                    15, false, "https://spdx.org/licenses/Afmparse.html#licenseText", "Afmparse License", false,
                    "Afmparse", null
                },
                {
                    14, true, "https://spdx.org/licenses/AFL-3.0.html#licenseText", "Academic Free License v3.0", true,
                    "AFL-3.0", null
                },
                {
                    13, true, "https://spdx.org/licenses/AFL-2.1.html#licenseText", "Academic Free License v2.1", true,
                    "AFL-2.1", null
                },
                {
                    12, true, "https://spdx.org/licenses/AFL-2.0.html#licenseText", "Academic Free License v2.0", true,
                    "AFL-2.0", null
                },
                {
                    11, true, "https://spdx.org/licenses/AFL-1.2.html#licenseText", "Academic Free License v1.2", true,
                    "AFL-1.2", null
                },
                {
                    10, true, "https://spdx.org/licenses/AFL-1.1.html#licenseText", "Academic Free License v1.1", true,
                    "AFL-1.1", null
                },
                {
                    9, false, "https://spdx.org/licenses/ADSL.html#licenseText", "Amazon Digital Services License",
                    false, "ADSL", null
                },
                {
                    8, false, "https://spdx.org/licenses/Adobe-Glyph.html#licenseText", "Adobe Glyph List License",
                    false, "Adobe-Glyph", null
                },
                {
                    7, false, "https://spdx.org/licenses/Adobe-2006.html#licenseText",
                    "Adobe Systems Incorporated Source Code License Agreement", false, "Adobe-2006", null
                },
                {
                    6, false, "https://spdx.org/licenses/Abstyles.html#licenseText", "Abstyles License", false,
                    "Abstyles", null
                },
                {
                    5, false, "https://spdx.org/licenses/AAL.html#licenseText", "Attribution Assurance License", true,
                    "AAL", null
                },
                {
                    4, false, "https://spdx.org/licenses/0BSD.html#licenseText", "BSD Zero Clause License", true,
                    "0BSD", null
                },
                {
                    3, false, null, "All rights reserved", false, null, null
                },
                {
                    2, false, null, "Advertisement use", false, null, null
                },
                {
                    21, false, "https://spdx.org/licenses/AMDPLPA.html#licenseText", "AMD's plpa_map.c License", false,
                    "AMDPLPA", null
                },
                {
                    89, false, "https://spdx.org/licenses/CC-BY-ND-4.0.html#licenseText",
                    "Creative Commons Attribution No Derivatives 4.0 International", false, "CC-BY-ND-4.0", null
                },
                {
                    22, false, "https://spdx.org/licenses/AML.html#licenseText", "Apple MIT License", false, "AML", null
                },
                {
                    24, false, "https://spdx.org/licenses/ANTLR-PD.html#licenseText", "ANTLR Software Rights Notice",
                    false, "ANTLR-PD", null
                },
                {
                    43, false, "https://spdx.org/licenses/Borceux.html#licenseText", "Borceux license", false,
                    "Borceux", null
                },
                {
                    42, true, "https://spdx.org/licenses/BitTorrent-1.1.html#licenseText",
                    "BitTorrent Open Source License v1.1", false, "BitTorrent-1.1", null
                },
                {
                    41, false, "https://spdx.org/licenses/BitTorrent-1.0.html#licenseText",
                    "BitTorrent Open Source License v1.0", false, "BitTorrent-1.0", null
                },
                {
                    40, false, "https://spdx.org/licenses/Beerware.html#licenseText", "Beerware License", false,
                    "Beerware", null
                },
                {
                    39, false, "https://spdx.org/licenses/Barr.html#licenseText", "Barr License", false, "Barr", null
                },
                {
                    38, false, "https://spdx.org/licenses/Bahyph.html#licenseText", "Bahyph License", false, "Bahyph",
                    null
                },
                {
                    37, true, "https://spdx.org/licenses/Artistic-2.0.html#licenseText", "Artistic License 2.0", true,
                    "Artistic-2.0", null
                },
                {
                    36, false, "https://spdx.org/licenses/Artistic-1.0-Perl.html#licenseText",
                    "Artistic License 1.0 (Perl)", true, "Artistic-1.0-Perl", null
                },
                {
                    35, false, "https://spdx.org/licenses/Artistic-1.0-cl8.html#licenseText",
                    "Artistic License 1.0 w/clause 8", true, "Artistic-1.0-cl8", null
                },
                {
                    34, false, "https://spdx.org/licenses/Artistic-1.0.html#licenseText", "Artistic License 1.0", true,
                    "Artistic-1.0", null
                },
                {
                    33, true, "https://spdx.org/licenses/APSL-2.0.html#licenseText", "Apple Public Source License 2.0",
                    true, "APSL-2.0", null
                },
                {
                    32, false, "https://spdx.org/licenses/APSL-1.2.html#licenseText", "Apple Public Source License 1.2",
                    true, "APSL-1.2", null
                },
                {
                    31, false, "https://spdx.org/licenses/APSL-1.1.html#licenseText", "Apple Public Source License 1.1",
                    true, "APSL-1.1", null
                },
                {
                    30, false, "https://spdx.org/licenses/APSL-1.0.html#licenseText", "Apple Public Source License 1.0",
                    true, "APSL-1.0", null
                },
                {
                    29, false, "https://spdx.org/licenses/APL-1.0.html#licenseText", "Adaptive Public License 1.0",
                    true, "APL-1.0", null
                },
                {
                    28, false, "https://spdx.org/licenses/APAFML.html#licenseText", "Adobe Postscript AFM License",
                    false, "APAFML", null
                },
                {
                    27, true, "https://spdx.org/licenses/Apache-2.0.html#licenseText", "Apache License 2.0", true,
                    "Apache-2.0", null
                },
                {
                    26, true, "https://spdx.org/licenses/Apache-1.1.html#licenseText", "Apache License 1.1", true,
                    "Apache-1.1", null
                },
                {
                    25, true, "https://spdx.org/licenses/Apache-1.0.html#licenseText", "Apache License 1.0", false,
                    "Apache-1.0", null
                },
                {
                    23, false, "https://spdx.org/licenses/AMPAS.html#licenseText",
                    "Academy of Motion Picture Arts and Sciences BSD", false, "AMPAS", null
                },
                {
                    181, true, "https://spdx.org/licenses/IPA.html#licenseText", "IPA Font License", true, "IPA", null
                },
                {
                    90, false, "https://spdx.org/licenses/CC-BY-SA-1.0.html#licenseText",
                    "Creative Commons Attribution Share Alike 1.0 Generic", false, "CC-BY-SA-1.0", null
                },
                {
                    92, false, "https://spdx.org/licenses/CC-BY-SA-2.5.html#licenseText",
                    "Creative Commons Attribution Share Alike 2.5 Generic", false, "CC-BY-SA-2.5", null
                },
                {
                    156, false, "https://spdx.org/licenses/Giftware.html#licenseText", "Giftware License", false,
                    "Giftware", null
                },
                {
                    155, true, "https://spdx.org/licenses/GFDL-1.3-or-later.html#licenseText",
                    "GNU Free Documentation License v1.3 or later", false, "GFDL-1.3-or-later", null
                },
                {
                    154, true, "https://spdx.org/licenses/GFDL-1.3-only.html#licenseText",
                    "GNU Free Documentation License v1.3 only", false, "GFDL-1.3-only", null
                },
                {
                    153, true, "https://spdx.org/licenses/GFDL-1.2-or-later.html#licenseText",
                    "GNU Free Documentation License v1.2 or later", false, "GFDL-1.2-or-later", null
                },
                {
                    152, true, "https://spdx.org/licenses/GFDL-1.2-only.html#licenseText",
                    "GNU Free Documentation License v1.2 only", false, "GFDL-1.2-only", null
                },
                {
                    151, true, "https://spdx.org/licenses/GFDL-1.1-or-later.html#licenseText",
                    "GNU Free Documentation License v1.1 or later", false, "GFDL-1.1-or-later", null
                },
                {
                    150, true, "https://spdx.org/licenses/GFDL-1.1-only.html#licenseText",
                    "GNU Free Documentation License v1.1 only", false, "GFDL-1.1-only", null
                },
                {
                    149, true, "https://spdx.org/licenses/FTL.html#licenseText", "Freetype Project License", false,
                    "FTL", null
                },
                {
                    157, false, "https://spdx.org/licenses/GL2PS.html#licenseText", "GL2PS License", false, "GL2PS",
                    null
                },
                {
                    148, false, "https://spdx.org/licenses/FSFULLR.html#licenseText",
                    "FSF Unlimited License (with License Retention)", false, "FSFULLR", null
                },
                {
                    146, true, "https://spdx.org/licenses/FSFAP.html#licenseText", "FSF All Permissive License", false,
                    "FSFAP", null
                },
                {
                    145, false, "https://spdx.org/licenses/FreeImage.html#licenseText", "FreeImage Public License v1.0",
                    false, "FreeImage", null
                },
                {
                    144, false, "https://spdx.org/licenses/Frameworx-1.0.html#licenseText",
                    "Frameworx Open License 1.0", true, "Frameworx-1.0", null
                },
                {
                    143, false, "https://spdx.org/licenses/Fair.html#licenseText", "Fair License", true, "Fair", null
                },
                {
                    142, false, "https://spdx.org/licenses/Eurosym.html#licenseText", "Eurosym License", false,
                    "Eurosym", null
                },
                {
                    141, true, "https://spdx.org/licenses/EUPL-1.2.html#licenseText",
                    "European Union Public License 1.2", true, "EUPL-1.2", null
                },
                {
                    140, true, "https://spdx.org/licenses/EUPL-1.1.html#licenseText",
                    "European Union Public License 1.1", true, "EUPL-1.1", null
                },
                {
                    139, false, "https://spdx.org/licenses/EUPL-1.0.html#licenseText",
                    "European Union Public License 1.0", false, "EUPL-1.0", null
                },
                {
                    147, false, "https://spdx.org/licenses/FSFUL.html#licenseText", "FSF Unlimited License", false,
                    "FSFUL", null
                },
                {
                    158, false, "https://spdx.org/licenses/Glide.html#licenseText", "3dfx Glide License", false,
                    "Glide", null
                },
                {
                    159, false, "https://spdx.org/licenses/Glulxe.html#licenseText", "Glulxe License", false, "Glulxe",
                    null
                },
                {
                    160, true, "https://spdx.org/licenses/gnuplot.html#licenseText", "gnuplot License", false,
                    "gnuplot", null
                },
                {
                    179, false, "https://spdx.org/licenses/Intel-ACPI.html#licenseText",
                    "Intel ACPI Software License Agreement", false, "Intel-ACPI", null
                },
                {
                    178, true, "https://spdx.org/licenses/Intel.html#licenseText", "Intel Open Source License", true,
                    "Intel", null
                },
                {
                    177, false, "https://spdx.org/licenses/Info-ZIP.html#licenseText", "Info-ZIP License", false,
                    "Info-ZIP", null
                }
            });

            migrationBuilder.InsertData("Licenses", new[]
            {
                "Id", "FsfApproved", "Link", "Name", "OsiApproved", "SPDX", "Text"
            }, new object[,]
            {
                {
                    176, true, "https://spdx.org/licenses/Imlib2.html#licenseText", "Imlib2 License", false, "Imlib2",
                    null
                },
                {
                    175, true, "https://spdx.org/licenses/iMatix.html#licenseText",
                    "iMatix Standard Function Library Agreement", false, "iMatix", null
                },
                {
                    174, false, "https://spdx.org/licenses/ImageMagick.html#licenseText", "ImageMagick License", false,
                    "ImageMagick", null
                },
                {
                    173, true, "https://spdx.org/licenses/IJG.html#licenseText", "Independent JPEG Group License",
                    false, "IJG", null
                },
                {
                    172, false, "https://spdx.org/licenses/ICU.html#licenseText", "ICU License", false, "ICU", null
                },
                {
                    171, false, "https://spdx.org/licenses/IBM-pibs.html#licenseText",
                    "IBM PowerPC Initialization and Boot Software", false, "IBM-pibs", null
                },
                {
                    170, false, "https://spdx.org/licenses/HPND-sell-variant.html#licenseText",
                    "Historical Permission Notice and Disclaimer - sell variant", false, "HPND-sell-variant", null
                },
                {
                    169, true, "https://spdx.org/licenses/HPND.html#licenseText",
                    "Historical Permission Notice and Disclaimer", true, "HPND", null
                },
                {
                    168, false, "https://spdx.org/licenses/HaskellReport.html#licenseText",
                    "Haskell Language Report License", false, "HaskellReport", null
                },
                {
                    167, false, "https://spdx.org/licenses/gSOAP-1.3b.html#licenseText", "gSOAP Public License v1.3b",
                    false, "gSOAP-1.3b", null
                },
                {
                    166, true, "https://spdx.org/licenses/GPL-3.0-or-later.html#licenseText",
                    "GNU General Public License v3.0 or later", true, "GPL-3.0-or-later", null
                },
                {
                    165, true, "https://spdx.org/licenses/GPL-3.0-only.html#licenseText",
                    "GNU General Public License v3.0 only", true, "GPL-3.0-only", null
                },
                {
                    164, true, "https://spdx.org/licenses/GPL-2.0-or-later.html#licenseText",
                    "GNU General Public License v2.0 or later", true, "GPL-2.0-or-later", null
                },
                {
                    163, true, "https://spdx.org/licenses/GPL-2.0-only.html#licenseText",
                    "GNU General Public License v2.0 only", true, "GPL-2.0-only", null
                },
                {
                    162, false, "https://spdx.org/licenses/GPL-1.0-or-later.html#licenseText",
                    "GNU General Public License v1.0 or later", false, "GPL-1.0-or-later", null
                },
                {
                    161, false, "https://spdx.org/licenses/GPL-1.0-only.html#licenseText",
                    "GNU General Public License v1.0 only", false, "GPL-1.0-only", null
                },
                {
                    138, true, "https://spdx.org/licenses/EUDatagrid.html#licenseText", "EU DataGrid Software License",
                    true, "EUDatagrid", null
                },
                {
                    137, false, "https://spdx.org/licenses/ErlPL-1.1.html#licenseText", "Erlang Public License v1.1",
                    false, "ErlPL-1.1", null
                },
                {
                    136, true, "https://spdx.org/licenses/EPL-2.0.html#licenseText", "Eclipse Public License 2.0", true,
                    "EPL-2.0", null
                },
                {
                    135, true, "https://spdx.org/licenses/EPL-1.0.html#licenseText", "Eclipse Public License 1.0", true,
                    "EPL-1.0", null
                },
                {
                    111, false, "https://spdx.org/licenses/CNRI-Python-GPL-Compatible.html#licenseText",
                    "CNRI Python Open Source GPL Compatible License Agreement", false, "CNRI-Python-GPL-Compatible",
                    null
                },
                {
                    110, false, "https://spdx.org/licenses/CNRI-Python.html#licenseText", "CNRI Python License", true,
                    "CNRI-Python", null
                },
                {
                    109, false, "https://spdx.org/licenses/CNRI-Jython.html#licenseText", "CNRI Jython License", false,
                    "CNRI-Jython", null
                },
                {
                    108, true, "https://spdx.org/licenses/ClArtistic.html#licenseText", "Clarified Artistic License",
                    false, "ClArtistic", null
                },
                {
                    107, false, "https://spdx.org/licenses/CERN-OHL-1.2.html#licenseText",
                    "CERN Open Hardware Licence v1.2", false, "CERN-OHL-1.2", null
                },
                {
                    106, false, "https://spdx.org/licenses/CERN-OHL-1.1.html#licenseText",
                    "CERN Open Hardware License v1.1", false, "CERN-OHL-1.1", null
                },
                {
                    105, true, "https://spdx.org/licenses/CECILL-C.html#licenseText",
                    "CeCILL-C Free Software License Agreement", false, "CECILL-C", null
                },
                {
                    104, true, "https://spdx.org/licenses/CECILL-B.html#licenseText",
                    "CeCILL-B Free Software License Agreement", false, "CECILL-B", null
                },
                {
                    103, false, "https://spdx.org/licenses/CECILL-2.1.html#licenseText",
                    "CeCILL Free Software License Agreement v2.1", true, "CECILL-2.1", null
                },
                {
                    102, true, "https://spdx.org/licenses/CECILL-2.0.html#licenseText",
                    "CeCILL Free Software License Agreement v2.0", false, "CECILL-2.0", null
                },
                {
                    101, false, "https://spdx.org/licenses/CECILL-1.1.html#licenseText",
                    "CeCILL Free Software License Agreement v1.1", false, "CECILL-1.1", null
                },
                {
                    100, false, "https://spdx.org/licenses/CECILL-1.0.html#licenseText",
                    "CeCILL Free Software License Agreement v1.0", false, "CECILL-1.0", null
                },
                {
                    99, false, "https://spdx.org/licenses/CDLA-Sharing-1.0.html#licenseText",
                    "Community Data License Agreement Sharing 1.0", false, "CDLA-Sharing-1.0", null
                },
                {
                    98, false, "https://spdx.org/licenses/CDLA-Permissive-1.0.html#licenseText",
                    "Community Data License Agreement Permissive 1.0", false, "CDLA-Permissive-1.0", null
                },
                {
                    97, false, "https://spdx.org/licenses/CDDL-1.1.html#licenseText",
                    "Common Development and Distribution License 1.1", false, "CDDL-1.1", null
                },
                {
                    96, true, "https://spdx.org/licenses/CDDL-1.0.html#licenseText",
                    "Common Development and Distribution License 1.0", true, "CDDL-1.0", null
                },
                {
                    95, true, "https://spdx.org/licenses/CC0-1.0.html#licenseText",
                    "Creative Commons Zero v1.0 Universal", false, "CC0-1.0", null
                },
                {
                    94, true, "https://spdx.org/licenses/CC-BY-SA-4.0.html#licenseText",
                    "Creative Commons Attribution Share Alike 4.0 International", false, "CC-BY-SA-4.0", null
                },
                {
                    93, false, "https://spdx.org/licenses/CC-BY-SA-3.0.html#licenseText",
                    "Creative Commons Attribution Share Alike 3.0 Unported", false, "CC-BY-SA-3.0", null
                },
                {
                    112, true, "https://spdx.org/licenses/Condor-1.1.html#licenseText", "Condor Public License v1.1",
                    false, "Condor-1.1", null
                },
                {
                    91, false, "https://spdx.org/licenses/CC-BY-SA-2.0.html#licenseText",
                    "Creative Commons Attribution Share Alike 2.0 Generic", false, "CC-BY-SA-2.0", null
                },
                {
                    113, false, "https://spdx.org/licenses/copyleft-next-0.3.0.html#licenseText", "copyleft-next 0.3.0",
                    false, "copyleft-next-0.3.0", null
                },
                {
                    115, true, "https://spdx.org/licenses/CPAL-1.0.html#licenseText",
                    "Common Public Attribution License 1.0", true, "CPAL-1.0", null
                },
                {
                    134, false, "https://spdx.org/licenses/Entessa.html#licenseText", "Entessa Public License v1.0",
                    true, "Entessa", null
                },
                {
                    133, false, "https://spdx.org/licenses/eGenix.html#licenseText", "eGenix.com Public License 1.1.0",
                    false, "eGenix", null
                },
                {
                    132, true, "https://spdx.org/licenses/EFL-2.0.html#licenseText", "Eiffel Forum License v2.0", true,
                    "EFL-2.0", null
                },
                {
                    131, false, "https://spdx.org/licenses/EFL-1.0.html#licenseText", "Eiffel Forum License v1.0", true,
                    "EFL-1.0", null
                },
                {
                    130, true, "https://spdx.org/licenses/ECL-2.0.html#licenseText",
                    "Educational Community License v2.0", true, "ECL-2.0", null
                },
                {
                    129, false, "https://spdx.org/licenses/ECL-1.0.html#licenseText",
                    "Educational Community License v1.0", true, "ECL-1.0", null
                },
                {
                    128, false, "https://spdx.org/licenses/dvipdfm.html#licenseText", "dvipdfm License", false,
                    "dvipdfm", null
                },
                {
                    127, false, "https://spdx.org/licenses/DSDP.html#licenseText", "DSDP License", false, "DSDP", null
                },
                {
                    126, false, "https://spdx.org/licenses/Dotseqn.html#licenseText", "Dotseqn License", false,
                    "Dotseqn", null
                },
                {
                    125, false, "https://spdx.org/licenses/DOC.html#licenseText", "DOC License", false, "DOC", null
                },
                {
                    124, false, "https://spdx.org/licenses/diffmark.html#licenseText", "diffmark license", false,
                    "diffmark", null
                },
                {
                    123, false, "https://spdx.org/licenses/D-FSL-1.0.html#licenseText",
                    "Deutsche Freie Software Lizenz", false, "D-FSL-1.0", null
                },
                {
                    122, false, "https://spdx.org/licenses/curl.html#licenseText", "curl License", false, "curl", null
                },
                {
                    121, false, "https://spdx.org/licenses/Cube.html#licenseText", "Cube License", false, "Cube", null
                },
                {
                    120, false, "https://spdx.org/licenses/CUA-OPL-1.0.html#licenseText",
                    "CUA Office Public License v1.0", true, "CUA-OPL-1.0", null
                },
                {
                    119, false, "https://spdx.org/licenses/CrystalStacker.html#licenseText", "CrystalStacker License",
                    false, "CrystalStacker", null
                },
                {
                    118, false, "https://spdx.org/licenses/Crossword.html#licenseText", "Crossword License", false,
                    "Crossword", null
                },
                {
                    117, false, "https://spdx.org/licenses/CPOL-1.02.html#licenseText",
                    "Code Project Open License 1.02", false, "CPOL-1.02", null
                },
                {
                    116, true, "https://spdx.org/licenses/CPL-1.0.html#licenseText", "Common Public License 1.0", true,
                    "CPL-1.0", null
                },
                {
                    114, false, "https://spdx.org/licenses/copyleft-next-0.3.1.html#licenseText", "copyleft-next 0.3.1",
                    false, "copyleft-next-0.3.1", null
                },
                {
                    363, true, "https://spdx.org/licenses/ZPL-2.1.html#licenseText", "Zope Public License 2.1", false,
                    "ZPL-2.1", null
                }
            });

            migrationBuilder.CreateIndex("IX_Licenses_FsfApproved", "Licenses", "FsfApproved");

            migrationBuilder.CreateIndex("IX_Licenses_Name", "Licenses", "Name");

            migrationBuilder.CreateIndex("IX_Licenses_OsiApproved", "Licenses", "OsiApproved");

            migrationBuilder.CreateIndex("IX_Licenses_SPDX", "Licenses", "SPDX");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Licenses");
    }
}