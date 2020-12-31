﻿/******************************************************************************
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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using Microsoft.AspNetCore.Mvc;

namespace Marechai.Database.Models
{
    public class ResolutionsByScreen : BaseModel<long>
    {
        [Remote("VerifyUnique", "ResolutionsByScreen", "Admin", AdditionalFields = nameof(ResolutionId))]
        public int ScreenId { get; set; }
        [Remote("VerifyUnique", "ResolutionsByScreen", "Admin", AdditionalFields = nameof(ScreenId))]
        public int ResolutionId { get; set; }

        public virtual Screen     Screen     { get; set; }
        public virtual Resolution Resolution { get; set; }
    }
}