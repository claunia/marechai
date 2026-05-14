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

using System;
using System.Text.RegularExpressions;

namespace Marechai.Server.Helpers;

/// <summary>
///     Server-side mirror of <c>Marechai.Helpers.YouTubeUrlParser</c> (Marechai.Server cannot
///     reference the Blazor project where the original lives). Same regex set and contract:
///     accepts any of <c>youtube.com/watch?v=</c>, <c>youtu.be/</c>,
///     <c>youtube.com/embed/</c>, <c>youtube.com/shorts/</c>, <c>youtube.com/live/</c>,
///     <c>youtube-nocookie.com</c> URLs (with or without <c>https://</c> and
///     <c>www./m./music.</c> prefixes), or a bare 11-character video ID. Returns the
///     extracted ID when match succeeds.
/// </summary>
public static partial class YouTubeUrlParser
{
    [GeneratedRegex(@"^(?:https?://)?(?:www\.|m\.|music\.)?(?:youtube\.com|youtube-nocookie\.com)/(?:watch\?(?:[^&\s]*&)*v=|embed/|v/|shorts/|live/)([A-Za-z0-9_-]{11})",
                    RegexOptions.IgnoreCase)]
    private static partial Regex YouTubeFullUrlRegex();

    [GeneratedRegex(@"^(?:https?://)?youtu\.be/([A-Za-z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YouTubeShortUrlRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_-]{11}$")]
    private static partial Regex BareIdRegex();

    /// <summary>
    ///     Tries to extract a YouTube video ID from a URL or a bare 11-character ID.
    /// </summary>
    public static bool TryExtract(string input, out string videoId)
    {
        videoId = null;

        if(string.IsNullOrWhiteSpace(input)) return false;

        string trimmed = input.Trim();

        Match match = YouTubeFullUrlRegex().Match(trimmed);

        if(!match.Success) match = YouTubeShortUrlRegex().Match(trimmed);

        if(match.Success)
        {
            videoId = match.Groups[1].Value;
            return true;
        }

        if(!BareIdRegex().IsMatch(trimmed)) return false;

        videoId = trimmed;
        return true;
    }
}
