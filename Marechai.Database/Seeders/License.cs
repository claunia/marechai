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

using System;
using System.Collections.Generic;
using System.Linq;
using Marechai.Database.Models;

namespace Marechai.Database.Seeders
{
    public static class License
    {
        // Last updated 31 May 2020 using SPDX License List v3.9
        public static void Seed(MarechaiContext context)
        {
            List<Models.License> existingLicenses     = context.Licenses.ToList();
            List<Models.License> newLicenses          = new List<Models.License>();
            int                  updatedLicencesCount = 0;

            foreach(Models.License license in new[]
            {
                new Models.License
                {
                    Id          = 1,
                    Name        = "Fair use",
                    FsfApproved = false,
                    OsiApproved = false
                },
                new Models.License
                {
                    Id          = 2,
                    Name        = "Advertisement use",
                    FsfApproved = false,
                    OsiApproved = false
                },
                new Models.License
                {
                    Id          = 3,
                    Name        = "All rights reserved",
                    FsfApproved = false,
                    OsiApproved = false
                },
                new Models.License
                {
                    Id          = 4,
                    Name        = "BSD Zero Clause License",
                    SPDX        = "0BSD",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/0BSD.html#licenseText"
                },
                new Models.License
                {
                    Id          = 5,
                    Name        = "Attribution Assurance License",
                    SPDX        = "AAL",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AAL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 6,
                    Name        = "Abstyles License",
                    SPDX        = "Abstyles",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Abstyles.html#licenseText"
                },
                new Models.License
                {
                    Id          = 7,
                    Name        = "Adobe Systems Incorporated Source Code License Agreement",
                    SPDX        = "Adobe-2006",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Adobe-2006.html#licenseText"
                },
                new Models.License
                {
                    Id          = 8,
                    Name        = "Adobe Glyph List License",
                    SPDX        = "Adobe-Glyph",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Adobe-Glyph.html#licenseText"
                },
                new Models.License
                {
                    Id          = 9,
                    Name        = "Amazon Digital Services License",
                    SPDX        = "ADSL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ADSL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 10,
                    Name        = "Academic Free License v1.1",
                    SPDX        = "AFL-1.1",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AFL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 11,
                    Name        = "Academic Free License v1.2",
                    SPDX        = "AFL-1.2",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AFL-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 12,
                    Name        = "Academic Free License v2.0",
                    SPDX        = "AFL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AFL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 13,
                    Name        = "Academic Free License v2.1",
                    SPDX        = "AFL-2.1",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AFL-2.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 14,
                    Name        = "Academic Free License v3.0",
                    SPDX        = "AFL-3.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AFL-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 15,
                    Name        = "Afmparse License",
                    SPDX        = "Afmparse",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Afmparse.html#licenseText"
                },
                new Models.License
                {
                    Id          = 16,
                    Name        = "Affero General Public License v1.0 only",
                    SPDX        = "AGPL-1.0-only",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/AGPL-1.0-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 17,
                    Name        = "Affero General Public License v1.0 or later",
                    SPDX        = "AGPL-1.0-or-later",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/AGPL-1.0-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 18,
                    Name        = "GNU Affero General Public License v3.0 only",
                    SPDX        = "AGPL-3.0-only",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AGPL-3.0-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 19,
                    Name        = "GNU Affero General Public License v3.0 or later",
                    SPDX        = "AGPL-3.0-or-later",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/AGPL-3.0-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 20,
                    Name        = "Aladdin Free Public License",
                    SPDX        = "Aladdin",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Aladdin.html#licenseText"
                },
                new Models.License
                {
                    Id          = 21,
                    Name        = "AMD's plpa_map.c License",
                    SPDX        = "AMDPLPA",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/AMDPLPA.html#licenseText"
                },
                new Models.License
                {
                    Id          = 22,
                    Name        = "Apple MIT License",
                    SPDX        = "AML",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/AML.html#licenseText"
                },
                new Models.License
                {
                    Id          = 23,
                    Name        = "Academy of Motion Picture Arts and Sciences BSD",
                    SPDX        = "AMPAS",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/AMPAS.html#licenseText"
                },
                new Models.License
                {
                    Id          = 24,
                    Name        = "ANTLR Software Rights Notice",
                    SPDX        = "ANTLR-PD",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ANTLR-PD.html#licenseText"
                },
                new Models.License
                {
                    Id          = 25,
                    Name        = "Apache License 1.0",
                    SPDX        = "Apache-1.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Apache-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 26,
                    Name        = "Apache License 1.1",
                    SPDX        = "Apache-1.1",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Apache-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 27,
                    Name        = "Apache License 2.0",
                    SPDX        = "Apache-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Apache-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 28,
                    Name        = "Adobe Postscript AFM License",
                    SPDX        = "APAFML",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/APAFML.html#licenseText"
                },
                new Models.License
                {
                    Id          = 29,
                    Name        = "Adaptive Public License 1.0",
                    SPDX        = "APL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/APL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 30,
                    Name        = "Apple Public Source License 1.0",
                    SPDX        = "APSL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/APSL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 31,
                    Name        = "Apple Public Source License 1.1",
                    SPDX        = "APSL-1.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/APSL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 32,
                    Name        = "Apple Public Source License 1.2",
                    SPDX        = "APSL-1.2",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/APSL-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 33,
                    Name        = "Apple Public Source License 2.0",
                    SPDX        = "APSL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/APSL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 34,
                    Name        = "Artistic License 1.0",
                    SPDX        = "Artistic-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Artistic-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 35,
                    Name        = "Artistic License 1.0 w/clause 8",
                    SPDX        = "Artistic-1.0-cl8",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Artistic-1.0-cl8.html#licenseText"
                },
                new Models.License
                {
                    Id          = 36,
                    Name        = "Artistic License 1.0 (Perl)",
                    SPDX        = "Artistic-1.0-Perl",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Artistic-1.0-Perl.html#licenseText"
                },
                new Models.License
                {
                    Id          = 37,
                    Name        = "Artistic License 2.0",
                    SPDX        = "Artistic-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Artistic-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 38,
                    Name        = "Bahyph License",
                    SPDX        = "Bahyph",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Bahyph.html#licenseText"
                },
                new Models.License
                {
                    Id          = 39,
                    Name        = "Barr License",
                    SPDX        = "Barr",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Barr.html#licenseText"
                },
                new Models.License
                {
                    Id          = 40,
                    Name        = "Beerware License",
                    SPDX        = "Beerware",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Beerware.html#licenseText"
                },
                new Models.License
                {
                    Id          = 41,
                    Name        = "BitTorrent Open Source License v1.0",
                    SPDX        = "BitTorrent-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BitTorrent-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 42,
                    Name        = "BitTorrent Open Source License v1.1",
                    SPDX        = "BitTorrent-1.1",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BitTorrent-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 43,
                    Name        = "Borceux license",
                    SPDX        = "Borceux",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Borceux.html#licenseText"
                },
                new Models.License
                {
                    Id          = 44,
                    Name        = "BSD 1-Clause License",
                    SPDX        = "BSD-1-Clause",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-1-Clause.html#licenseText"
                },
                new Models.License
                {
                    Id          = 45,
                    Name        = "BSD 2-Clause \"Simplified\" License",
                    SPDX        = "BSD-2-Clause",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/BSD-2-Clause.html#licenseText"
                },
                new Models.License
                {
                    Id          = 46,
                    Name        = "BSD 2-Clause FreeBSD License",
                    SPDX        = "BSD-2-Clause-FreeBSD",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-2-Clause-FreeBSD.html#licenseText"
                },
                new Models.License
                {
                    Id          = 47,
                    Name        = "BSD 2-Clause NetBSD License",
                    SPDX        = "BSD-2-Clause-NetBSD",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-2-Clause-NetBSD.html#licenseText"
                },
                new Models.License
                {
                    Id          = 48,
                    Name        = "BSD-2-Clause Plus Patent License",
                    SPDX        = "BSD-2-Clause-Patent",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/BSD-2-Clause-Patent.html#licenseText"
                },
                new Models.License
                {
                    Id          = 49,
                    Name        = "BSD 3-Clause \"New\" or \"Revised\" License",
                    SPDX        = "BSD-3-Clause",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/BSD-3-Clause.html#licenseText"
                },
                new Models.License
                {
                    Id          = 50,
                    Name        = "BSD with attribution",
                    SPDX        = "BSD-3-Clause-Attribution",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-3-Clause-Attribution.html#licenseText"
                },
                new Models.License
                {
                    Id          = 51,
                    Name        = "BSD 3-Clause Clear License",
                    SPDX        = "BSD-3-Clause-Clear",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-3-Clause-Clear.html#licenseText"
                },
                new Models.License
                {
                    Id          = 52,
                    Name        = "Lawrence Berkeley National Labs BSD variant license",
                    SPDX        = "BSD-3-Clause-LBNL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-3-Clause-LBNL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 53,
                    Name        = "BSD 3-Clause No Nuclear License",
                    SPDX        = "BSD-3-Clause-No-Nuclear-License",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-3-Clause-No-Nuclear-License.html#licenseText"
                },
                new Models.License
                {
                    Id          = 54,
                    Name        = "BSD 3-Clause No Nuclear License 2014",
                    SPDX        = "BSD-3-Clause-No-Nuclear-License-2014",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-3-Clause-No-Nuclear-License-2014.html#licenseText"
                },
                new Models.License
                {
                    Id          = 55,
                    Name        = "BSD 3-Clause No Nuclear Warranty",
                    SPDX        = "BSD-3-Clause-No-Nuclear-Warranty",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-3-Clause-No-Nuclear-Warranty.html#licenseText"
                },
                new Models.License
                {
                    Id          = 56,
                    Name        = "BSD 4-Clause \"Original\" or \"Old\" License",
                    SPDX        = "BSD-4-Clause",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-4-Clause.html#licenseText"
                },
                new Models.License
                {
                    Id          = 57,
                    Name        = "BSD-4-Clause (University of California-Specific)",
                    SPDX        = "BSD-4-Clause-UC",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-4-Clause-UC.html#licenseText"
                },
                new Models.License
                {
                    Id          = 58,
                    Name        = "BSD Protection License",
                    SPDX        = "BSD-Protection",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-Protection.html#licenseText"
                },
                new Models.License
                {
                    Id          = 59,
                    Name        = "BSD Source Code Attribution",
                    SPDX        = "BSD-Source-Code",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/BSD-Source-Code.html#licenseText"
                },
                new Models.License
                {
                    Id          = 60,
                    Name        = "Boost Software License 1.0",
                    SPDX        = "BSL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/BSL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 61,
                    Name        = "bzip2 and libbzip2 License v1.0.5",
                    SPDX        = "bzip2-1.0.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/bzip2-1.0.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 62,
                    Name        = "bzip2 and libbzip2 License v1.0.6",
                    SPDX        = "bzip2-1.0.6",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/bzip2-1.0.6.html#licenseText"
                },
                new Models.License
                {
                    Id          = 63,
                    Name        = "Caldera License",
                    SPDX        = "Caldera",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Caldera.html#licenseText"
                },
                new Models.License
                {
                    Id          = 64,
                    Name        = "Computer Associates Trusted Open Source License 1.1",
                    SPDX        = "CATOSL-1.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/CATOSL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 65,
                    Name        = "Creative Commons Attribution 1.0 Generic",
                    SPDX        = "CC-BY-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 66,
                    Name        = "Creative Commons Attribution 2.0 Generic",
                    SPDX        = "CC-BY-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 67,
                    Name        = "Creative Commons Attribution 2.5 Generic",
                    SPDX        = "CC-BY-2.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-2.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 68,
                    Name        = "Creative Commons Attribution 3.0 Unported",
                    SPDX        = "CC-BY-3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 69,
                    Name        = "Creative Commons Attribution 4.0 International",
                    SPDX        = "CC-BY-4.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-4.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 70,
                    Name        = "Creative Commons Attribution Non Commercial 1.0 Generic",
                    SPDX        = "CC-BY-NC-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 71,
                    Name        = "Creative Commons Attribution Non Commercial 2.0 Generic",
                    SPDX        = "CC-BY-NC-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 72,
                    Name        = "Creative Commons Attribution Non Commercial 2.5 Generic",
                    SPDX        = "CC-BY-NC-2.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-2.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 73,
                    Name        = "Creative Commons Attribution Non Commercial 3.0 Unported",
                    SPDX        = "CC-BY-NC-3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 74,
                    Name        = "Creative Commons Attribution Non Commercial 4.0 International",
                    SPDX        = "CC-BY-NC-4.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-4.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 75,
                    Name        = "Creative Commons Attribution Non Commercial No Derivatives 1.0 Generic",
                    SPDX        = "CC-BY-NC-ND-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-ND-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 76,
                    Name        = "Creative Commons Attribution Non Commercial No Derivatives 2.0 Generic",
                    SPDX        = "CC-BY-NC-ND-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-ND-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 77,
                    Name        = "Creative Commons Attribution Non Commercial No Derivatives 2.5 Generic",
                    SPDX        = "CC-BY-NC-ND-2.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-ND-2.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 78,
                    Name        = "Creative Commons Attribution Non Commercial No Derivatives 3.0 Unported",
                    SPDX        = "CC-BY-NC-ND-3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-ND-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 79,
                    Name        = "Creative Commons Attribution Non Commercial No Derivatives 4.0 International",
                    SPDX        = "CC-BY-NC-ND-4.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-ND-4.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 80,
                    Name        = "Creative Commons Attribution Non Commercial Share Alike 1.0 Generic",
                    SPDX        = "CC-BY-NC-SA-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-SA-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 81,
                    Name        = "Creative Commons Attribution Non Commercial Share Alike 2.0 Generic",
                    SPDX        = "CC-BY-NC-SA-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-SA-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 82,
                    Name        = "Creative Commons Attribution Non Commercial Share Alike 2.5 Generic",
                    SPDX        = "CC-BY-NC-SA-2.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-SA-2.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 83,
                    Name        = "Creative Commons Attribution Non Commercial Share Alike 3.0 Unported",
                    SPDX        = "CC-BY-NC-SA-3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-SA-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 84,
                    Name        = "Creative Commons Attribution Non Commercial Share Alike 4.0 International",
                    SPDX        = "CC-BY-NC-SA-4.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-NC-SA-4.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 85,
                    Name        = "Creative Commons Attribution No Derivatives 1.0 Generic",
                    SPDX        = "CC-BY-ND-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-ND-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 86,
                    Name        = "Creative Commons Attribution No Derivatives 2.0 Generic",
                    SPDX        = "CC-BY-ND-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-ND-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 87,
                    Name        = "Creative Commons Attribution No Derivatives 2.5 Generic",
                    SPDX        = "CC-BY-ND-2.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-ND-2.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 88,
                    Name        = "Creative Commons Attribution No Derivatives 3.0 Unported",
                    SPDX        = "CC-BY-ND-3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-ND-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 89,
                    Name        = "Creative Commons Attribution No Derivatives 4.0 International",
                    SPDX        = "CC-BY-ND-4.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-ND-4.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 90,
                    Name        = "Creative Commons Attribution Share Alike 1.0 Generic",
                    SPDX        = "CC-BY-SA-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-SA-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 91,
                    Name        = "Creative Commons Attribution Share Alike 2.0 Generic",
                    SPDX        = "CC-BY-SA-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-SA-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 92,
                    Name        = "Creative Commons Attribution Share Alike 2.5 Generic",
                    SPDX        = "CC-BY-SA-2.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-SA-2.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 93,
                    Name        = "Creative Commons Attribution Share Alike 3.0 Unported",
                    SPDX        = "CC-BY-SA-3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-SA-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 94,
                    Name        = "Creative Commons Attribution Share Alike 4.0 International",
                    SPDX        = "CC-BY-SA-4.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC-BY-SA-4.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 95,
                    Name        = "Creative Commons Zero v1.0 Universal",
                    SPDX        = "CC0-1.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CC0-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 96,
                    Name        = "Common Development and Distribution License 1.0",
                    SPDX        = "CDDL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/CDDL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 97,
                    Name        = "Common Development and Distribution License 1.1",
                    SPDX        = "CDDL-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CDDL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 98,
                    Name        = "Community Data License Agreement Permissive 1.0",
                    SPDX        = "CDLA-Permissive-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CDLA-Permissive-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 99,
                    Name        = "Community Data License Agreement Sharing 1.0",
                    SPDX        = "CDLA-Sharing-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CDLA-Sharing-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 100,
                    Name        = "CeCILL Free Software License Agreement v1.0",
                    SPDX        = "CECILL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CECILL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 101,
                    Name        = "CeCILL Free Software License Agreement v1.1",
                    SPDX        = "CECILL-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CECILL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 102,
                    Name        = "CeCILL Free Software License Agreement v2.0",
                    SPDX        = "CECILL-2.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CECILL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 103,
                    Name        = "CeCILL Free Software License Agreement v2.1",
                    SPDX        = "CECILL-2.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/CECILL-2.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 104,
                    Name        = "CeCILL-B Free Software License Agreement",
                    SPDX        = "CECILL-B",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CECILL-B.html#licenseText"
                },
                new Models.License
                {
                    Id          = 105,
                    Name        = "CeCILL-C Free Software License Agreement",
                    SPDX        = "CECILL-C",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CECILL-C.html#licenseText"
                },
                new Models.License
                {
                    Id          = 106,
                    Name        = "CERN Open Hardware License v1.1",
                    SPDX        = "CERN-OHL-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CERN-OHL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 107,
                    Name        = "CERN Open Hardware Licence v1.2",
                    SPDX        = "CERN-OHL-1.2",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CERN-OHL-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 108,
                    Name        = "Clarified Artistic License",
                    SPDX        = "ClArtistic",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ClArtistic.html#licenseText"
                },
                new Models.License
                {
                    Id          = 109,
                    Name        = "CNRI Jython License",
                    SPDX        = "CNRI-Jython",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CNRI-Jython.html#licenseText"
                },
                new Models.License
                {
                    Id          = 110,
                    Name        = "CNRI Python License",
                    SPDX        = "CNRI-Python",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/CNRI-Python.html#licenseText"
                },
                new Models.License
                {
                    Id          = 111,
                    Name        = "CNRI Python Open Source GPL Compatible License Agreement",
                    SPDX        = "CNRI-Python-GPL-Compatible",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CNRI-Python-GPL-Compatible.html#licenseText"
                },
                new Models.License
                {
                    Id          = 112,
                    Name        = "Condor Public License v1.1",
                    SPDX        = "Condor-1.1",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Condor-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 113,
                    Name        = "copyleft-next 0.3.0",
                    SPDX        = "copyleft-next-0.3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/copyleft-next-0.3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 114,
                    Name        = "copyleft-next 0.3.1",
                    SPDX        = "copyleft-next-0.3.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/copyleft-next-0.3.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 115,
                    Name        = "Common Public Attribution License 1.0",
                    SPDX        = "CPAL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/CPAL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 116,
                    Name        = "Common Public License 1.0",
                    SPDX        = "CPL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/CPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 117,
                    Name        = "Code Project Open License 1.02",
                    SPDX        = "CPOL-1.02",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CPOL-1.02.html#licenseText"
                },
                new Models.License
                {
                    Id          = 118,
                    Name        = "Crossword License",
                    SPDX        = "Crossword",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Crossword.html#licenseText"
                },
                new Models.License
                {
                    Id          = 119,
                    Name        = "CrystalStacker License",
                    SPDX        = "CrystalStacker",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CrystalStacker.html#licenseText"
                },
                new Models.License
                {
                    Id          = 120,
                    Name        = "CUA Office Public License v1.0",
                    SPDX        = "CUA-OPL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/CUA-OPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 121,
                    Name        = "Cube License",
                    SPDX        = "Cube",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Cube.html#licenseText"
                },
                new Models.License
                {
                    Id          = 122,
                    Name        = "curl License",
                    SPDX        = "curl",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/curl.html#licenseText"
                },
                new Models.License
                {
                    Id          = 123,
                    Name        = "Deutsche Freie Software Lizenz",
                    SPDX        = "D-FSL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/D-FSL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 124,
                    Name        = "diffmark license",
                    SPDX        = "diffmark",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/diffmark.html#licenseText"
                },
                new Models.License
                {
                    Id          = 125,
                    Name        = "DOC License",
                    SPDX        = "DOC",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/DOC.html#licenseText"
                },
                new Models.License
                {
                    Id          = 126,
                    Name        = "Dotseqn License",
                    SPDX        = "Dotseqn",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Dotseqn.html#licenseText"
                },
                new Models.License
                {
                    Id          = 127,
                    Name        = "DSDP License",
                    SPDX        = "DSDP",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/DSDP.html#licenseText"
                },
                new Models.License
                {
                    Id          = 128,
                    Name        = "dvipdfm License",
                    SPDX        = "dvipdfm",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/dvipdfm.html#licenseText"
                },
                new Models.License
                {
                    Id          = 129,
                    Name        = "Educational Community License v1.0",
                    SPDX        = "ECL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/ECL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 130,
                    Name        = "Educational Community License v2.0",
                    SPDX        = "ECL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/ECL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 131,
                    Name        = "Eiffel Forum License v1.0",
                    SPDX        = "EFL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/EFL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 132,
                    Name        = "Eiffel Forum License v2.0",
                    SPDX        = "EFL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/EFL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 133,
                    Name        = "eGenix.com Public License 1.1.0",
                    SPDX        = "eGenix",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/eGenix.html#licenseText"
                },
                new Models.License
                {
                    Id          = 134,
                    Name        = "Entessa Public License v1.0",
                    SPDX        = "Entessa",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Entessa.html#licenseText"
                },
                new Models.License
                {
                    Id          = 135,
                    Name        = "Eclipse Public License 1.0",
                    SPDX        = "EPL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/EPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 136,
                    Name        = "Eclipse Public License 2.0",
                    SPDX        = "EPL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/EPL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 137,
                    Name        = "Erlang Public License v1.1",
                    SPDX        = "ErlPL-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ErlPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 138,
                    Name        = "EU DataGrid Software License",
                    SPDX        = "EUDatagrid",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/EUDatagrid.html#licenseText"
                },
                new Models.License
                {
                    Id          = 139,
                    Name        = "European Union Public License 1.0",
                    SPDX        = "EUPL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/EUPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 140,
                    Name        = "European Union Public License 1.1",
                    SPDX        = "EUPL-1.1",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/EUPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 141,
                    Name        = "European Union Public License 1.2",
                    SPDX        = "EUPL-1.2",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/EUPL-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 142,
                    Name        = "Eurosym License",
                    SPDX        = "Eurosym",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Eurosym.html#licenseText"
                },
                new Models.License
                {
                    Id          = 143,
                    Name        = "Fair License",
                    SPDX        = "Fair",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Fair.html#licenseText"
                },
                new Models.License
                {
                    Id          = 144,
                    Name        = "Frameworx Open License 1.0",
                    SPDX        = "Frameworx-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Frameworx-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 145,
                    Name        = "FreeImage Public License v1.0",
                    SPDX        = "FreeImage",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/FreeImage.html#licenseText"
                },
                new Models.License
                {
                    Id          = 146,
                    Name        = "FSF All Permissive License",
                    SPDX        = "FSFAP",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/FSFAP.html#licenseText"
                },
                new Models.License
                {
                    Id          = 147,
                    Name        = "FSF Unlimited License",
                    SPDX        = "FSFUL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/FSFUL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 148,
                    Name        = "FSF Unlimited License (with License Retention)",
                    SPDX        = "FSFULLR",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/FSFULLR.html#licenseText"
                },
                new Models.License
                {
                    Id          = 149,
                    Name        = "Freetype Project License",
                    SPDX        = "FTL",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/FTL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 150,
                    Name        = "GNU Free Documentation License v1.1 only",
                    SPDX        = "GFDL-1.1-only",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GFDL-1.1-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 151,
                    Name        = "GNU Free Documentation License v1.1 or later",
                    SPDX        = "GFDL-1.1-or-later",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GFDL-1.1-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 152,
                    Name        = "GNU Free Documentation License v1.2 only",
                    SPDX        = "GFDL-1.2-only",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GFDL-1.2-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 153,
                    Name        = "GNU Free Documentation License v1.2 or later",
                    SPDX        = "GFDL-1.2-or-later",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GFDL-1.2-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 154,
                    Name        = "GNU Free Documentation License v1.3 only",
                    SPDX        = "GFDL-1.3-only",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GFDL-1.3-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 155,
                    Name        = "GNU Free Documentation License v1.3 or later",
                    SPDX        = "GFDL-1.3-or-later",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GFDL-1.3-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 156,
                    Name        = "Giftware License",
                    SPDX        = "Giftware",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Giftware.html#licenseText"
                },
                new Models.License
                {
                    Id          = 157,
                    Name        = "GL2PS License",
                    SPDX        = "GL2PS",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GL2PS.html#licenseText"
                },
                new Models.License
                {
                    Id          = 158,
                    Name        = "3dfx Glide License",
                    SPDX        = "Glide",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Glide.html#licenseText"
                },
                new Models.License
                {
                    Id          = 159,
                    Name        = "Glulxe License",
                    SPDX        = "Glulxe",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Glulxe.html#licenseText"
                },
                new Models.License
                {
                    Id          = 160,
                    Name        = "gnuplot License",
                    SPDX        = "gnuplot",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/gnuplot.html#licenseText"
                },
                new Models.License
                {
                    Id          = 161,
                    Name        = "GNU General Public License v1.0 only",
                    SPDX        = "GPL-1.0-only",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GPL-1.0-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 162,
                    Name        = "GNU General Public License v1.0 or later",
                    SPDX        = "GPL-1.0-or-later",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/GPL-1.0-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 163,
                    Name        = "GNU General Public License v2.0 only",
                    SPDX        = "GPL-2.0-only",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/GPL-2.0-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 164,
                    Name        = "GNU General Public License v2.0 or later",
                    SPDX        = "GPL-2.0-or-later",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/GPL-2.0-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 165,
                    Name        = "GNU General Public License v3.0 only",
                    SPDX        = "GPL-3.0-only",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/GPL-3.0-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 166,
                    Name        = "GNU General Public License v3.0 or later",
                    SPDX        = "GPL-3.0-or-later",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/GPL-3.0-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 167,
                    Name        = "gSOAP Public License v1.3b",
                    SPDX        = "gSOAP-1.3b",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/gSOAP-1.3b.html#licenseText"
                },
                new Models.License
                {
                    Id          = 168,
                    Name        = "Haskell Language Report License",
                    SPDX        = "HaskellReport",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/HaskellReport.html#licenseText"
                },
                new Models.License
                {
                    Id          = 169,
                    Name        = "Historical Permission Notice and Disclaimer",
                    SPDX        = "HPND",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/HPND.html#licenseText"
                },
                new Models.License
                {
                    Id          = 170,
                    Name        = "Historical Permission Notice and Disclaimer - sell variant",
                    SPDX        = "HPND-sell-variant",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/HPND-sell-variant.html#licenseText"
                },
                new Models.License
                {
                    Id          = 171,
                    Name        = "IBM PowerPC Initialization and Boot Software",
                    SPDX        = "IBM-pibs",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/IBM-pibs.html#licenseText"
                },
                new Models.License
                {
                    Id          = 172,
                    Name        = "ICU License",
                    SPDX        = "ICU",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ICU.html#licenseText"
                },
                new Models.License
                {
                    Id          = 173,
                    Name        = "Independent JPEG Group License",
                    SPDX        = "IJG",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/IJG.html#licenseText"
                },
                new Models.License
                {
                    Id          = 174,
                    Name        = "ImageMagick License",
                    SPDX        = "ImageMagick",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ImageMagick.html#licenseText"
                },
                new Models.License
                {
                    Id          = 175,
                    Name        = "iMatix Standard Function Library Agreement",
                    SPDX        = "iMatix",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/iMatix.html#licenseText"
                },
                new Models.License
                {
                    Id          = 176,
                    Name        = "Imlib2 License",
                    SPDX        = "Imlib2",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Imlib2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 177,
                    Name        = "Info-ZIP License",
                    SPDX        = "Info-ZIP",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Info-ZIP.html#licenseText"
                },
                new Models.License
                {
                    Id          = 178,
                    Name        = "Intel Open Source License",
                    SPDX        = "Intel",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Intel.html#licenseText"
                },
                new Models.License
                {
                    Id          = 179,
                    Name        = "Intel ACPI Software License Agreement",
                    SPDX        = "Intel-ACPI",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Intel-ACPI.html#licenseText"
                },
                new Models.License
                {
                    Id          = 180,
                    Name        = "Interbase Public License v1.0",
                    SPDX        = "Interbase-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Interbase-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 181,
                    Name        = "IPA Font License",
                    SPDX        = "IPA",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/IPA.html#licenseText"
                },
                new Models.License
                {
                    Id          = 182,
                    Name        = "IBM Public License v1.0",
                    SPDX        = "IPL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/IPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 183,
                    Name        = "ISC License",
                    SPDX        = "ISC",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/ISC.html#licenseText"
                },
                new Models.License
                {
                    Id          = 184,
                    Name        = "JasPer License",
                    SPDX        = "JasPer-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/JasPer-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 185,
                    Name        = "Japan Network Information Center License",
                    SPDX        = "JPNIC",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/JPNIC.html#licenseText"
                },
                new Models.License
                {
                    Id          = 186,
                    Name        = "JSON License",
                    SPDX        = "JSON",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/JSON.html#licenseText"
                },
                new Models.License
                {
                    Id          = 187,
                    Name        = "Licence Art Libre 1.2",
                    SPDX        = "LAL-1.2",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LAL-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 188,
                    Name        = "Licence Art Libre 1.3",
                    SPDX        = "LAL-1.3",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LAL-1.3.html#licenseText"
                },
                new Models.License
                {
                    Id          = 189,
                    Name        = "Latex2e License",
                    SPDX        = "Latex2e",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Latex2e.html#licenseText"
                },
                new Models.License
                {
                    Id          = 190,
                    Name        = "Leptonica License",
                    SPDX        = "Leptonica",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Leptonica.html#licenseText"
                },
                new Models.License
                {
                    Id          = 191,
                    Name        = "GNU Library General Public License v2 only",
                    SPDX        = "LGPL-2.0-only",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LGPL-2.0-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 192,
                    Name        = "GNU Library General Public License v2 or later",
                    SPDX        = "LGPL-2.0-or-later",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LGPL-2.0-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 193,
                    Name        = "GNU Lesser General Public License v2.1 only",
                    SPDX        = "LGPL-2.1-only",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LGPL-2.1-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 194,
                    Name        = "GNU Lesser General Public License v2.1 or later",
                    SPDX        = "LGPL-2.1-or-later",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LGPL-2.1-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 195,
                    Name        = "GNU Lesser General Public License v3.0 only",
                    SPDX        = "LGPL-3.0-only",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LGPL-3.0-only.html#licenseText"
                },
                new Models.License
                {
                    Id          = 196,
                    Name        = "GNU Lesser General Public License v3.0 or later",
                    SPDX        = "LGPL-3.0-or-later",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LGPL-3.0-or-later.html#licenseText"
                },
                new Models.License
                {
                    Id          = 197,
                    Name        = "Lesser General Public License For Linguistic Resources",
                    SPDX        = "LGPLLR",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LGPLLR.html#licenseText"
                },
                new Models.License
                {
                    Id          = 198,
                    Name        = "libpng License",
                    SPDX        = "Libpng",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Libpng.html#licenseText"
                },
                new Models.License
                {
                    Id          = 199,
                    Name        = "PNG Reference Library version 2",
                    SPDX        = "libpng-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/libpng-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 200,
                    Name        = "libtiff License",
                    SPDX        = "libtiff",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/libtiff.html#licenseText"
                },
                new Models.License
                {
                    Id          = 201,
                    Name        = "Licence Libre du Québec – Permissive version 1.1",
                    SPDX        = "LiLiQ-P-1.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LiLiQ-P-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 202,
                    Name        = "Licence Libre du Québec – Réciprocité version 1.1",
                    SPDX        = "LiLiQ-R-1.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LiLiQ-R-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 203,
                    Name        = "Licence Libre du Québec – Réciprocité forte version 1.1",
                    SPDX        = "LiLiQ-Rplus-1.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LiLiQ-Rplus-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 204,
                    Name        = "Linux Kernel Variant of OpenIB.org license",
                    SPDX        = "Linux-OpenIB",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Linux-OpenIB.html#licenseText"
                },
                new Models.License
                {
                    Id          = 205,
                    Name        = "Lucent Public License Version 1.0",
                    SPDX        = "LPL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 206,
                    Name        = "Lucent Public License v1.02",
                    SPDX        = "LPL-1.02",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LPL-1.02.html#licenseText"
                },
                new Models.License
                {
                    Id          = 207,
                    Name        = "LaTeX Project Public License v1.0",
                    SPDX        = "LPPL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LPPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 208,
                    Name        = "LaTeX Project Public License v1.1",
                    SPDX        = "LPPL-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LPPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 209,
                    Name        = "LaTeX Project Public License v1.2",
                    SPDX        = "LPPL-1.2",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LPPL-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 210,
                    Name        = "LaTeX Project Public License v1.3a",
                    SPDX        = "LPPL-1.3a",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LPPL-1.3a.html#licenseText"
                },
                new Models.License
                {
                    Id          = 211,
                    Name        = "LaTeX Project Public License v1.3c",
                    SPDX        = "LPPL-1.3c",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/LPPL-1.3c.html#licenseText"
                },
                new Models.License
                {
                    Id          = 212,
                    Name        = "MakeIndex License",
                    SPDX        = "MakeIndex",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MakeIndex.html#licenseText"
                },
                new Models.License
                {
                    Id          = 213,
                    Name        = "MirOS License",
                    SPDX        = "MirOS",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MirOS.html#licenseText"
                },
                new Models.License
                {
                    Id          = 214,
                    Name        = "MIT License",
                    SPDX        = "MIT",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MIT.html#licenseText"
                },
                new Models.License
                {
                    Id          = 215,
                    Name        = "MIT No Attribution",
                    SPDX        = "MIT-0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MIT-0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 216,
                    Name        = "Enlightenment License (e16)",
                    SPDX        = "MIT-advertising",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MIT-advertising.html#licenseText"
                },
                new Models.License
                {
                    Id          = 217,
                    Name        = "CMU License",
                    SPDX        = "MIT-CMU",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MIT-CMU.html#licenseText"
                },
                new Models.License
                {
                    Id          = 218,
                    Name        = "enna License",
                    SPDX        = "MIT-enna",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MIT-enna.html#licenseText"
                },
                new Models.License
                {
                    Id          = 219,
                    Name        = "feh License",
                    SPDX        = "MIT-feh",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MIT-feh.html#licenseText"
                },
                new Models.License
                {
                    Id          = 220,
                    Name        = "MIT +no-false-attribs license",
                    SPDX        = "MITNFA",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MITNFA.html#licenseText"
                },
                new Models.License
                {
                    Id          = 221,
                    Name        = "Motosoto License",
                    SPDX        = "Motosoto",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Motosoto.html#licenseText"
                },
                new Models.License
                {
                    Id          = 222,
                    Name        = "mpich2 License",
                    SPDX        = "mpich2",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/mpich2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 223,
                    Name        = "Mozilla Public License 1.0",
                    SPDX        = "MPL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 224,
                    Name        = "Mozilla Public License 1.1",
                    SPDX        = "MPL-1.1",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 225,
                    Name        = "Mozilla Public License 2.0",
                    SPDX        = "MPL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MPL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 226,
                    Name        = "Mozilla Public License 2.0 (no copyleft exception)",
                    SPDX        = "MPL-2.0-no-copyleft-exception",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MPL-2.0-no-copyleft-exception.html#licenseText"
                },
                new Models.License
                {
                    Id          = 227,
                    Name        = "Microsoft Public License",
                    SPDX        = "MS-PL",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MS-PL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 228,
                    Name        = "Microsoft Reciprocal License",
                    SPDX        = "MS-RL",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/MS-RL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 229,
                    Name        = "Matrix Template Library License",
                    SPDX        = "MTLL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MTLL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 230,
                    Name        = "Multics License",
                    SPDX        = "Multics",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Multics.html#licenseText"
                },
                new Models.License
                {
                    Id          = 231,
                    Name        = "Mup License",
                    SPDX        = "Mup",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Mup.html#licenseText"
                },
                new Models.License
                {
                    Id          = 232,
                    Name        = "NASA Open Source Agreement 1.3",
                    SPDX        = "NASA-1.3",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/NASA-1.3.html#licenseText"
                },
                new Models.License
                {
                    Id          = 233,
                    Name        = "Naumen Public License",
                    SPDX        = "Naumen",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Naumen.html#licenseText"
                },
                new Models.License
                {
                    Id          = 234,
                    Name        = "Net Boolean Public License v1",
                    SPDX        = "NBPL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NBPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 235,
                    Name        = "University of Illinois/NCSA Open Source License",
                    SPDX        = "NCSA",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/NCSA.html#licenseText"
                },
                new Models.License
                {
                    Id          = 236,
                    Name        = "Net-SNMP License",
                    SPDX        = "Net-SNMP",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Net-SNMP.html#licenseText"
                },
                new Models.License
                {
                    Id          = 237,
                    Name        = "NetCDF license",
                    SPDX        = "NetCDF",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NetCDF.html#licenseText"
                },
                new Models.License
                {
                    Id          = 238,
                    Name        = "Newsletr License",
                    SPDX        = "Newsletr",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Newsletr.html#licenseText"
                },
                new Models.License
                {
                    Id          = 239,
                    Name        = "Nethack General Public License",
                    SPDX        = "NGPL",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/NGPL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 240,
                    Name        = "Norwegian Licence for Open Government Data",
                    SPDX        = "NLOD-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NLOD-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 241,
                    Name        = "No Limit Public License",
                    SPDX        = "NLPL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NLPL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 242,
                    Name        = "Nokia Open Source License",
                    SPDX        = "Nokia",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Nokia.html#licenseText"
                },
                new Models.License
                {
                    Id          = 243,
                    Name        = "Netizen Open Source License",
                    SPDX        = "NOSL",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NOSL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 244,
                    Name        = "Noweb License",
                    SPDX        = "Noweb",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Noweb.html#licenseText"
                },
                new Models.License
                {
                    Id          = 245,
                    Name        = "Netscape Public License v1.0",
                    SPDX        = "NPL-1.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 246,
                    Name        = "Netscape Public License v1.1",
                    SPDX        = "NPL-1.1",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 247,
                    Name        = "Non-Profit Open Software License 3.0",
                    SPDX        = "NPOSL-3.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/NPOSL-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 248,
                    Name        = "NRL License",
                    SPDX        = "NRL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NRL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 249,
                    Name        = "NTP License",
                    SPDX        = "NTP",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/NTP.html#licenseText"
                },
                new Models.License
                {
                    Id          = 250,
                    Name        = "Open CASCADE Technology Public License",
                    SPDX        = "OCCT-PL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OCCT-PL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 251,
                    Name        = "OCLC Research Public License 2.0",
                    SPDX        = "OCLC-2.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OCLC-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 252,
                    Name        = "ODC Open Database License v1.0",
                    SPDX        = "ODbL-1.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ODbL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 253,
                    Name        = "Open Data Commons Attribution License v1.0",
                    SPDX        = "ODC-By-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ODC-By-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 254,
                    Name        = "SIL Open Font License 1.0",
                    SPDX        = "OFL-1.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OFL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 255,
                    Name        = "SIL Open Font License 1.1",
                    SPDX        = "OFL-1.1",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OFL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 256,
                    Name        = "Open Government Licence v1.0",
                    SPDX        = "OGL-UK-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OGL-UK-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 257,
                    Name        = "Open Government Licence v2.0",
                    SPDX        = "OGL-UK-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OGL-UK-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 258,
                    Name        = "Open Government Licence v3.0",
                    SPDX        = "OGL-UK-3.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OGL-UK-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 259,
                    Name        = "Open Group Test Suite License",
                    SPDX        = "OGTSL",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OGTSL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 260,
                    Name        = "Open LDAP Public License v1.1",
                    SPDX        = "OLDAP-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 261,
                    Name        = "Open LDAP Public License v1.2",
                    SPDX        = "OLDAP-1.2",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 262,
                    Name        = "Open LDAP Public License v1.3",
                    SPDX        = "OLDAP-1.3",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-1.3.html#licenseText"
                },
                new Models.License
                {
                    Id          = 263,
                    Name        = "Open LDAP Public License v1.4",
                    SPDX        = "OLDAP-1.4",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-1.4.html#licenseText"
                },
                new Models.License
                {
                    Id          = 264,
                    Name        = "Open LDAP Public License v2.0 (or possibly 2.0A and 2.0B)",
                    SPDX        = "OLDAP-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 265,
                    Name        = "Open LDAP Public License v2.0.1",
                    SPDX        = "OLDAP-2.0.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.0.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 266,
                    Name        = "Open LDAP Public License v2.1",
                    SPDX        = "OLDAP-2.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 267,
                    Name        = "Open LDAP Public License v2.2",
                    SPDX        = "OLDAP-2.2",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 268,
                    Name        = "Open LDAP Public License v2.2.1",
                    SPDX        = "OLDAP-2.2.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.2.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 269,
                    Name        = "Open LDAP Public License 2.2.2",
                    SPDX        = "OLDAP-2.2.2",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.2.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 270,
                    Name        = "Open LDAP Public License v2.3",
                    SPDX        = "OLDAP-2.3",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.3.html#licenseText"
                },
                new Models.License
                {
                    Id          = 271,
                    Name        = "Open LDAP Public License v2.4",
                    SPDX        = "OLDAP-2.4",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.4.html#licenseText"
                },
                new Models.License
                {
                    Id          = 272,
                    Name        = "Open LDAP Public License v2.5",
                    SPDX        = "OLDAP-2.5",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 273,
                    Name        = "Open LDAP Public License v2.6",
                    SPDX        = "OLDAP-2.6",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.6.html#licenseText"
                },
                new Models.License
                {
                    Id          = 274,
                    Name        = "Open LDAP Public License v2.7",
                    SPDX        = "OLDAP-2.7",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.7.html#licenseText"
                },
                new Models.License
                {
                    Id          = 275,
                    Name        = "Open LDAP Public License v2.8",
                    SPDX        = "OLDAP-2.8",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OLDAP-2.8.html#licenseText"
                },
                new Models.License
                {
                    Id          = 276,
                    Name        = "Open Market License",
                    SPDX        = "OML",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OML.html#licenseText"
                },
                new Models.License
                {
                    Id          = 277,
                    Name        = "OpenSSL License",
                    SPDX        = "OpenSSL",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OpenSSL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 278,
                    Name        = "Open Public License v1.0",
                    SPDX        = "OPL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 279,
                    Name        = "OSET Public License version 2.1",
                    SPDX        = "OSET-PL-2.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OSET-PL-2.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 280,
                    Name        = "Open Software License 1.0",
                    SPDX        = "OSL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OSL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 281,
                    Name        = "Open Software License 1.1",
                    SPDX        = "OSL-1.1",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OSL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 282,
                    Name        = "Open Software License 2.0",
                    SPDX        = "OSL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OSL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 283,
                    Name        = "Open Software License 2.1",
                    SPDX        = "OSL-2.1",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OSL-2.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 284,
                    Name        = "Open Software License 3.0",
                    SPDX        = "OSL-3.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/OSL-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 285,
                    Name        = "ODC Public Domain Dedication & License 1.0",
                    SPDX        = "PDDL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/PDDL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 286,
                    Name        = "PHP License v3.0",
                    SPDX        = "PHP-3.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/PHP-3.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 287,
                    Name        = "PHP License v3.01",
                    SPDX        = "PHP-3.01",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/PHP-3.01.html#licenseText"
                },
                new Models.License
                {
                    Id          = 288,
                    Name        = "Plexus Classworlds License",
                    SPDX        = "Plexus",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Plexus.html#licenseText"
                },
                new Models.License
                {
                    Id          = 289,
                    Name        = "PostgreSQL License",
                    SPDX        = "PostgreSQL",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/PostgreSQL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 290,
                    Name        = "psfrag License",
                    SPDX        = "psfrag",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/psfrag.html#licenseText"
                },
                new Models.License
                {
                    Id          = 291,
                    Name        = "psutils License",
                    SPDX        = "psutils",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/psutils.html#licenseText"
                },
                new Models.License
                {
                    Id          = 292,
                    Name        = "Python License 2.0",
                    SPDX        = "Python-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Python-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 293,
                    Name        = "Qhull License",
                    SPDX        = "Qhull",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Qhull.html#licenseText"
                },
                new Models.License
                {
                    Id          = 294,
                    Name        = "Q Public License 1.0",
                    SPDX        = "QPL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/QPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 295,
                    Name        = "Rdisc License",
                    SPDX        = "Rdisc",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Rdisc.html#licenseText"
                },
                new Models.License
                {
                    Id          = 296,
                    Name        = "Red Hat eCos Public License v1.1",
                    SPDX        = "RHeCos-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/RHeCos-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 297,
                    Name        = "Reciprocal Public License 1.1",
                    SPDX        = "RPL-1.1",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/RPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 298,
                    Name        = "Reciprocal Public License 1.5",
                    SPDX        = "RPL-1.5",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/RPL-1.5.html#licenseText"
                },
                new Models.License
                {
                    Id          = 299,
                    Name        = "RealNetworks Public Source License v1.0",
                    SPDX        = "RPSL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/RPSL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 300,
                    Name        = "RSA Message-Digest License",
                    SPDX        = "RSA-MD",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/RSA-MD.html#licenseText"
                },
                new Models.License
                {
                    Id          = 301,
                    Name        = "Ricoh Source Code Public License",
                    SPDX        = "RSCPL",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/RSCPL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 302,
                    Name        = "Ruby License",
                    SPDX        = "Ruby",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Ruby.html#licenseText"
                },
                new Models.License
                {
                    Id          = 303,
                    Name        = "Sax Public Domain Notice",
                    SPDX        = "SAX-PD",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SAX-PD.html#licenseText"
                },
                new Models.License
                {
                    Id          = 304,
                    Name        = "Saxpath License",
                    SPDX        = "Saxpath",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Saxpath.html#licenseText"
                },
                new Models.License
                {
                    Id          = 305,
                    Name        = "SCEA Shared Source License",
                    SPDX        = "SCEA",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SCEA.html#licenseText"
                },
                new Models.License
                {
                    Id          = 306,
                    Name        = "Sendmail License",
                    SPDX        = "Sendmail",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Sendmail.html#licenseText"
                },
                new Models.License
                {
                    Id          = 307,
                    Name        = "Sendmail License 8.23",
                    SPDX        = "Sendmail-8.23",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Sendmail-8.23.html#licenseText"
                },
                new Models.License
                {
                    Id          = 308,
                    Name        = "SGI Free Software License B v1.0",
                    SPDX        = "SGI-B-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SGI-B-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 309,
                    Name        = "SGI Free Software License B v1.1",
                    SPDX        = "SGI-B-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SGI-B-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 310,
                    Name        = "SGI Free Software License B v2.0",
                    SPDX        = "SGI-B-2.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SGI-B-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 311,
                    Name        = "Simple Public License 2.0",
                    SPDX        = "SimPL-2.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/SimPL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 312,
                    Name        = "Sun Industry Standards Source License v1.1",
                    SPDX        = "SISSL",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/SISSL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 313,
                    Name        = "Sun Industry Standards Source License v1.2",
                    SPDX        = "SISSL-1.2",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SISSL-1.2.html#licenseText"
                },
                new Models.License
                {
                    Id          = 314,
                    Name        = "Sleepycat License",
                    SPDX        = "Sleepycat",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Sleepycat.html#licenseText"
                },
                new Models.License
                {
                    Id          = 315,
                    Name        = "Standard ML of New Jersey License",
                    SPDX        = "SMLNJ",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SMLNJ.html#licenseText"
                },
                new Models.License
                {
                    Id          = 316,
                    Name        = "Secure Messaging Protocol Public License",
                    SPDX        = "SMPPL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SMPPL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 317,
                    Name        = "SNIA Public License 1.1",
                    SPDX        = "SNIA",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SNIA.html#licenseText"
                },
                new Models.License
                {
                    Id          = 318,
                    Name        = "Spencer License 86",
                    SPDX        = "Spencer-86",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Spencer-86.html#licenseText"
                },
                new Models.License
                {
                    Id          = 319,
                    Name        = "Spencer License 94",
                    SPDX        = "Spencer-94",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Spencer-94.html#licenseText"
                },
                new Models.License
                {
                    Id          = 320,
                    Name        = "Spencer License 99",
                    SPDX        = "Spencer-99",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Spencer-99.html#licenseText"
                },
                new Models.License
                {
                    Id          = 321,
                    Name        = "Sun Public License v1.0",
                    SPDX        = "SPL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/SPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 322,
                    Name        = "SugarCRM Public License v1.1.3",
                    SPDX        = "SugarCRM-1.1.3",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SugarCRM-1.1.3.html#licenseText"
                },
                new Models.License
                {
                    Id          = 323,
                    Name        = "Scheme Widget Library (SWL) Software License Agreement",
                    SPDX        = "SWL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SWL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 324,
                    Name        = "TAPR Open Hardware License v1.0",
                    SPDX        = "TAPR-OHL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TAPR-OHL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 325,
                    Name        = "TCL/TK License",
                    SPDX        = "TCL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TCL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 326,
                    Name        = "TCP Wrappers License",
                    SPDX        = "TCP-wrappers",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TCP-wrappers.html#licenseText"
                },
                new Models.License
                {
                    Id          = 327,
                    Name        = "TMate Open Source License",
                    SPDX        = "TMate",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TMate.html#licenseText"
                },
                new Models.License
                {
                    Id          = 328,
                    Name        = "TORQUE v2.5+ Software License v1.1",
                    SPDX        = "TORQUE-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TORQUE-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 329,
                    Name        = "Trusster Open Source License",
                    SPDX        = "TOSL",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TOSL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 330,
                    Name        = "Technische Universitaet Berlin License 1.0",
                    SPDX        = "TU-Berlin-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TU-Berlin-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 331,
                    Name        = "Technische Universitaet Berlin License 2.0",
                    SPDX        = "TU-Berlin-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/TU-Berlin-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 332,
                    Name        = "Unicode License Agreement - Data Files and Software (2015)",
                    SPDX        = "Unicode-DFS-2015",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Unicode-DFS-2015.html#licenseText"
                },
                new Models.License
                {
                    Id          = 333,
                    Name        = "Unicode License Agreement - Data Files and Software (2016)",
                    SPDX        = "Unicode-DFS-2016",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Unicode-DFS-2016.html#licenseText"
                },
                new Models.License
                {
                    Id          = 334,
                    Name        = "Unicode Terms of Use",
                    SPDX        = "Unicode-TOU",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Unicode-TOU.html#licenseText"
                },
                new Models.License
                {
                    Id          = 335,
                    Name        = "The Unlicense",
                    SPDX        = "Unlicense",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Unlicense.html#licenseText"
                },
                new Models.License
                {
                    Id          = 336,
                    Name        = "Universal Permissive License v1.0",
                    SPDX        = "UPL-1.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/UPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 337,
                    Name        = "Vim License",
                    SPDX        = "Vim",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Vim.html#licenseText"
                },
                new Models.License
                {
                    Id          = 338,
                    Name        = "VOSTROM Public License for Open Source",
                    SPDX        = "VOSTROM",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/VOSTROM.html#licenseText"
                },
                new Models.License
                {
                    Id          = 339,
                    Name        = "Vovida Software License v1.0",
                    SPDX        = "VSL-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/VSL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 340,
                    Name        = "W3C Software Notice and License (2002-12-31)",
                    SPDX        = "W3C",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/W3C.html#licenseText"
                },
                new Models.License
                {
                    Id          = 341,
                    Name        = "W3C Software Notice and License (1998-07-20)",
                    SPDX        = "W3C-19980720",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/W3C-19980720.html#licenseText"
                },
                new Models.License
                {
                    Id          = 342,
                    Name        = "W3C Software Notice and Document License (2015-05-13)",
                    SPDX        = "W3C-20150513",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/W3C-20150513.html#licenseText"
                },
                new Models.License
                {
                    Id          = 343,
                    Name        = "Sybase Open Watcom Public License 1.0",
                    SPDX        = "Watcom-1.0",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Watcom-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 344,
                    Name        = "Wsuipa License",
                    SPDX        = "Wsuipa",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Wsuipa.html#licenseText"
                },
                new Models.License
                {
                    Id          = 345,
                    Name        = "Do What The F*ck You Want To Public License",
                    SPDX        = "WTFPL",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/WTFPL.html#licenseText"
                },
                new Models.License
                {
                    Id          = 346,
                    Name        = "X11 License",
                    SPDX        = "X11",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/X11.html#licenseText"
                },
                new Models.License
                {
                    Id          = 347,
                    Name        = "Xerox License",
                    SPDX        = "Xerox",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Xerox.html#licenseText"
                },
                new Models.License
                {
                    Id          = 348,
                    Name        = "XFree86 License 1.1",
                    SPDX        = "XFree86-1.1",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/XFree86-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 349,
                    Name        = "xinetd License",
                    SPDX        = "xinetd",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/xinetd.html#licenseText"
                },
                new Models.License
                {
                    Id          = 350,
                    Name        = "X.Net License",
                    SPDX        = "Xnet",
                    FsfApproved = false,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Xnet.html#licenseText"
                },
                new Models.License
                {
                    Id          = 351,
                    Name        = "XPP License",
                    SPDX        = "xpp",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/xpp.html#licenseText"
                },
                new Models.License
                {
                    Id          = 352,
                    Name        = "XSkat License",
                    SPDX        = "XSkat",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/XSkat.html#licenseText"
                },
                new Models.License
                {
                    Id          = 353,
                    Name        = "Yahoo! Public License v1.0",
                    SPDX        = "YPL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/YPL-1.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 354,
                    Name        = "Yahoo! Public License v1.1",
                    SPDX        = "YPL-1.1",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/YPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 355,
                    Name        = "Zed License",
                    SPDX        = "Zed",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Zed.html#licenseText"
                },
                new Models.License
                {
                    Id          = 356,
                    Name        = "Zend License v2.0",
                    SPDX        = "Zend-2.0",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Zend-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 357,
                    Name        = "Zimbra Public License v1.3",
                    SPDX        = "Zimbra-1.3",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Zimbra-1.3.html#licenseText"
                },
                new Models.License
                {
                    Id          = 358,
                    Name        = "Zimbra Public License v1.4",
                    SPDX        = "Zimbra-1.4",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Zimbra-1.4.html#licenseText"
                },
                new Models.License
                {
                    Id          = 359,
                    Name        = "zlib License",
                    SPDX        = "Zlib",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/Zlib.html#licenseText"
                },
                new Models.License
                {
                    Id          = 360,
                    Name        = "zlib/libpng License with Acknowledgement",
                    SPDX        = "zlib-acknowledgement",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/zlib-acknowledgement.html#licenseText"
                },
                new Models.License
                {
                    Id          = 361,
                    Name        = "Zope Public License 1.1",
                    SPDX        = "ZPL-1.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ZPL-1.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 362,
                    Name        = "Zope Public License 2.0",
                    SPDX        = "ZPL-2.0",
                    FsfApproved = true,
                    OsiApproved = true,
                    Link        = "https://spdx.org/licenses/ZPL-2.0.html#licenseText"
                },
                new Models.License
                {
                    Id          = 363,
                    Name        = "Zope Public License 2.1",
                    SPDX        = "ZPL-2.1",
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/ZPL-2.1.html#licenseText"
                },
                new Models.License
                {
                    Id          = 364,
                    Name        = "Public domain",
                    SPDX        = null,
                    FsfApproved = true,
                    OsiApproved = false,
                    Link        = null
                },
                new Models.License
                {
                    Id          = 365,
                    Name        = "Cryptographic Autonomy License 1.0",
                    SPDX        = "CAL-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CAL-1.0.html"
                },
                new Models.License
                {
                    Id          = 366,
                    Name        = "Cryptographic Autonomy License 1.0 (Combined Work Exception)",
                    SPDX        = "CAL-1.0-Combined-Work-Exception",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CAL-1.0-Combined-Work-Exception.html"
                },
                new Models.License
                {
                    Id          = 367,
                    Name        = "CERN Open Hardware Licence Version 2 - Permissive",
                    SPDX        = "CERN-OHL-P-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CERN-OHL-P-2.0.html"
                },
                new Models.License
                {
                    Id          = 368,
                    Name        = "CERN Open Hardware Licence Version 2 - Strongly Reciprocal",
                    SPDX        = "CERN-OHL-S-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CERN-OHL-S-2.0.html"
                },
                new Models.License
                {
                    Id          = 369,
                    Name        = "CERN Open Hardware Licence Version 2 - Weakly Reciprocal",
                    SPDX        = "CERN-OHL-W-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/CERN-OHL-W-2.0.html"
                },
                new Models.License
                {
                    Id          = 370,
                    Name        = "Hippocratic License 2.1",
                    SPDX        = "Hippocratic-2.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Hippocratic-2.1.html"
                },
                new Models.License
                {
                    Id          = 371,
                    Name        = "LGPL-3.0 Linking Exception",
                    SPDX        = "LGPL-3.0-linking-exception",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/LGPL-3.0-linking-exception.html"
                },
                new Models.License
                {
                    Id          = 372,
                    Name        = "Mulan Permissive Software License, Version 2",
                    SPDX        = "MulanPSL-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/MulanPSL-2.0.html"
                },
                new Models.License
                {
                    Id          = 373,
                    Name        = "Non-Commercial Government Licence",
                    SPDX        = "NCGL-UK-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/NCGL-UK-2.0.html"
                },
                new Models.License
                {
                    Id          = 374,
                    Name        = "Open Use of Data Agreement v1.0",
                    SPDX        = "O-UDA-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/O-UDA-1.0.html"
                },
                new Models.License
                {
                    Id          = 375,
                    Name        = "OGC Software License, Version 1.0",
                    SPDX        = "OGC-1.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/OGC-1.0.html"
                },
                new Models.License
                {
                    Id          = 376,
                    Name        = "The Parity Public License 7.0.0",
                    SPDX        = "Parity-7.0.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/Parity-7.0.0.html"
                },
                new Models.License
                {
                    Id          = 377,
                    Name        = "PolyForm Noncommercial License 1.0.0",
                    SPDX        = "PolyForm-Noncommercial-1.0.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/PolyForm-Noncommercial-1.0.0.html"
                },
                new Models.License
                {
                    Id          = 378,
                    Name        = "PolyForm Small Business License 1.0.0",
                    SPDX        = "PolyForm-Small-Business-1.0.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/PolyForm-Small-Business-1.0.0.html"
                },
                new Models.License
                {
                    Id          = 379,
                    Name        = "Solderpad Hardware License v2.0",
                    SPDX        = "SHL-2.0",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SHL-2.0.html"
                },
                new Models.License
                {
                    Id          = 380,
                    Name        = "Solderpad Hardware License v2.1",
                    SPDX        = "SHL-2.1",
                    FsfApproved = false,
                    OsiApproved = false,
                    Link        = "https://spdx.org/licenses/SHL-2.1.html"
                }
            })
            {
                Models.License existingLicense = existingLicenses.FirstOrDefault(r => r.Id == license.Id);

                if(existingLicense is null)
                    newLicenses.Add(license);
                else
                {
                    bool changed = false;

                    if(license.Name != existingLicense.Name)
                    {
                        existingLicense.Name = license.Name;
                        changed              = true;
                    }

                    if(license.SPDX != existingLicense.SPDX)
                    {
                        existingLicense.SPDX = license.SPDX;
                        changed              = true;
                    }

                    if(license.FsfApproved != existingLicense.FsfApproved)
                    {
                        existingLicense.FsfApproved = license.FsfApproved;
                        changed                     = true;
                    }

                    if(license.OsiApproved != existingLicense.OsiApproved)
                    {
                        existingLicense.OsiApproved = license.OsiApproved;
                        changed                     = true;
                    }

                    if(license.Link != existingLicense.Link)
                    {
                        existingLicense.Link = license.Link;
                        changed              = true;
                    }

                    if(changed)
                        updatedLicencesCount++;
                }
            }

            context.Licenses.AddRange(newLicenses);

            Console.WriteLine("{0} licenses will be added.", newLicenses.Count);
            Console.WriteLine("{0} licenses will be updated.", updatedLicencesCount);
        }
    }
}