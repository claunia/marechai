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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

namespace Marechai.Services;

public sealed class IndexNowOptions
{
    public bool     Enabled   { get; set; }
    public string   Key       { get; set; }
    public string[] Endpoints { get; set; } =
    [
        "https://www.bing.com/indexnow",
        "https://yandex.com/indexnow",
        "https://search.seznam.cz/indexnow",
        "https://searchadvisor.naver.com/indexnow",
        "https://indexnow.yep.com/indexnow",
        "https://internetarchive.indexnow.org/indexnow",
        "https://indexnow.amazonbot.amazon/indexnow"
    ];
}
