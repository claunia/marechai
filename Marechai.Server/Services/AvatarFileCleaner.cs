/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Removes a user's stored avatar files from disk: the original upload plus all asynchronously
///     generated processed variants (per format × per size, both full and thumbnail). Used by
///     <c>AuthController.DELETE /auth/me/avatar</c> for individual avatar removal and by
///     <see cref="UserAccountDeletionService" /> on full-account purges. Errors deleting individual
///     files are swallowed and logged so a missing variant cannot block the parent operation.
/// </summary>
public sealed class AvatarFileCleaner(IConfiguration configuration, ILogger<AvatarFileCleaner> logger)
{
    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    public void DeleteAvatarFiles(Guid avatarGuid)
    {
        string photosRoot = Path.Combine(_assetRootPath, "photos", "avatars");

        // Delete original (any extension).
        string originalsDir = Path.Combine(photosRoot, "originals");

        if(Directory.Exists(originalsDir))
        {
            foreach(string f in Directory.GetFiles(originalsDir, $"{avatarGuid}.*"))
            {
                try { File.Delete(f); }
                catch(Exception ex) { logger.LogWarning(ex, "Failed to delete avatar original {Path}", f); }
            }
        }

        // Delete all generated variants (format × size × {full, thumb}).
        string[] formats = ["jpeg", "webp", "avif"];
        string[] sizes   = ["hd", "1440p", "4k"];

        foreach(string format in formats)
        {
            foreach(string size in sizes)
            {
                DeleteFilesIn(Path.Combine(photosRoot, format, size), $"{avatarGuid}.*");
                DeleteFilesIn(Path.Combine(photosRoot, "thumbs", format, size), $"{avatarGuid}.*");
            }
        }
    }

    void DeleteFilesIn(string directory, string pattern)
    {
        if(!Directory.Exists(directory)) return;

        foreach(string f in Directory.GetFiles(directory, pattern))
        {
            try { File.Delete(f); }
            catch(Exception ex) { logger.LogWarning(ex, "Failed to delete avatar variant {Path}", f); }
        }
    }
}
