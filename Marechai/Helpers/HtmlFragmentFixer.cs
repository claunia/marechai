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

namespace Marechai.Helpers;

/// <summary>
///     Rewrites fragment-only anchor links (<c>href="#..."</c>) so they include the current page path.
///     Without this, <c>&lt;base href="/"&gt;</c> causes fragment links in rendered markdown (e.g. Markdig
///     footnotes) to navigate to the site root instead of scrolling within the current page.
/// </summary>
public static class HtmlFragmentFixer
{
    public static string FixFragmentLinks(string html, string currentPath)
    {
        if(string.IsNullOrEmpty(html)) return html;

        return html.Replace("href=\"#", $"href=\"{currentPath}#");
    }
}
