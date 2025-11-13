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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class DumpDto : BaseDto<ulong>
{
    [JsonPropertyName("dumper")]
    [Required]
    public required string Dumper { get; set; }
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    [JsonPropertyName("dumping_group")]
    public string? DumpingGroup { get; set; }
    [JsonPropertyName("dump_date")]
    public DateTime? DumpDate { get; set; }
    [JsonPropertyName("username")]
    public string? UserName { get; set; }
    [JsonPropertyName("media_id")]
    public ulong MediaId { get; set; }
    [JsonPropertyName("media_title")]
    public string? MediaTitle { get; set; }
    [JsonPropertyName("media_dump_id")]
    public ulong MediaDumpId { get; set; }
}