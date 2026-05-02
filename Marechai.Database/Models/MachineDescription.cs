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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class MachineDescription : BaseModel<int>
{
    public int MachineId { get; set; }
    [StringLength(3)]
    [Required]
    public string LanguageCode { get; set; }
    [MaxLength(262144, ErrorMessage = "Description is too long")]
    [Required]
    public string Text { get; set; }
    [MaxLength(262144, ErrorMessage = "Description is too long")]
    [DisplayName("HTML")]
    public string Html { get; set; }

    public virtual Machine Machine  { get; set; }
    public virtual Iso639  Language { get; set; }
}
