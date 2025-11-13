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
using Aaru.CommonTypes;
using Marechai.Database;
using Marechai.Database.Models;

namespace Marechai.Data.Dtos;

public class MediaDto : BaseDto<ulong>
{
    [JsonPropertyName("title")]
    [Required]
    public required string Title { get; set; }
    [JsonPropertyName("sequence")]
    public ushort? Sequence { get; set; }
    [JsonPropertyName("last_sequence")]
    public ushort? LastSequence { get; set; }
    [JsonPropertyName("type")]
    public MediaType Type { get; set; }
    [JsonPropertyName("write_offset")]
    public int? WriteOffset { get; set; }
    [JsonPropertyName("sides")]
    public ushort? Sides { get; set; }
    [JsonPropertyName("layers")]
    public ushort? Layers { get; set; }
    [JsonPropertyName("sessions")]
    public ushort? Sessions { get; set; }
    [JsonPropertyName("tracks")]
    public ushort? Tracks { get; set; }
    [JsonPropertyName("sectors")]
    public ulong Sectors { get; set; }
    [JsonPropertyName("size")]
    public ulong Size { get; set; }
    [JsonPropertyName("copy_protection")]
    public string? CopyProtection { get; set; }
    [JsonPropertyName("part_number")]
    public string? PartNumber { get; set; }
    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; set; }
    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }
    [JsonPropertyName("catalogue_number")]
    public string? CatalogueNumber { get; set; }
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    [JsonPropertyName("revision")]
    public string? Revision { get; set; }
    [JsonPropertyName("firmware")]
    public string? Firmware { get; set; }
    [JsonPropertyName("physical_block_size")]
    public int? PhysicalBlockSize { get; set; }
    [JsonPropertyName("logical_block_size")]
    public int? LogicalBlockSize { get; set; }
    [JsonPropertyName("block_sizes")]
    public VariableBlockSize[]? BlockSizes { get; set; }
    [JsonPropertyName("storage_interface")]
    public StorageInterface? StorageInterface { get; set; }
    [JsonPropertyName("table_of_contents")]
    public OpticalDiscTrack[]? TableOfContents { get; set; }
}