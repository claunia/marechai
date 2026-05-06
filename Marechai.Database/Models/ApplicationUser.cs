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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string DisplayName { get; set; }

    [MaxLength(2000)]
    public string Bio { get; set; }

    [MaxLength(500)]
    public string Website { get; set; }

    [MaxLength(200)]
    public string Location { get; set; }

    public bool UseGravatar { get; set; } = true;

    public Guid? AvatarGuid { get; set; }

    [MaxLength(10)]
    public string OriginalAvatarExtension { get; set; }

    [MaxLength(100)]
    public string Twitter { get; set; }

    [MaxLength(100)]
    public string GitHub { get; set; }

    [MaxLength(200)]
    public string Mastodon { get; set; }

    [MaxLength(200)]
    public string Facebook { get; set; }

    [MaxLength(200)]
    public string LinkedIn { get; set; }

    public virtual ICollection<MachinePhoto>              Photos                    { get; set; }
    public virtual ICollection<OwnedMachine>              OwnedMachines             { get; set; }
    public virtual ICollection<CollectedBook>             CollectedBooks            { get; set; }
    public virtual ICollection<CollectedDocument>         CollectedDocuments        { get; set; }
    public virtual ICollection<CollectedSoftwareRelease>  CollectedSoftwareReleases { get; set; }
    public virtual ICollection<Dump>                      Dumps                     { get; set; }
    public virtual ICollection<BookScan>                  BookScans                 { get; set; }
    public virtual ICollection<DocumentScan>              DocumentScans             { get; set; }
    public virtual ICollection<MagazineScan>              MagazineScans             { get; set; }
    public virtual ICollection<SoftwareUserRating>        SoftwareRatings           { get; set; }
    public virtual ICollection<SoftwareUserReview>        SoftwareReviews           { get; set; }
    public virtual ICollection<SoftwareUserReviewVote>    ReviewVotes               { get; set; }
    public virtual ICollection<ReviewReport>              ReviewReports             { get; set; }
}