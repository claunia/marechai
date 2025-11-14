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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class MediaService(MarechaiContext context)
{
    public async Task<List<MediaDto>> GetAsync() => await context.Media.OrderBy(d => d.Title)
                                                                       .Select(d => new MediaDto
                                                                        {
                                                                            Id                = d.Id,
                                                                            Title             = d.Title,
                                                                            Sequence          = d.Sequence,
                                                                            LastSequence      = d.LastSequence,
                                                                            Type              = d.Type,
                                                                            WriteOffset       = d.WriteOffset,
                                                                            Sides             = d.Sides,
                                                                            Layers            = d.Layers,
                                                                            Sessions          = d.Sessions,
                                                                            Tracks            = d.Tracks,
                                                                            Sectors           = d.Sectors,
                                                                            Size              = d.Size,
                                                                            CopyProtection    = d.CopyProtection,
                                                                            PartNumber        = d.PartNumber,
                                                                            SerialNumber      = d.SerialNumber,
                                                                            Barcode           = d.Barcode,
                                                                            CatalogueNumber   = d.CatalogueNumber,
                                                                            Manufacturer      = d.Manufacturer,
                                                                            Model             = d.Model,
                                                                            Revision          = d.Revision,
                                                                            Firmware          = d.Firmware,
                                                                            PhysicalBlockSize = d.PhysicalBlockSize,
                                                                            LogicalBlockSize  = d.LogicalBlockSize,
                                                                            BlockSizes        = d.BlockSizes,
                                                                            StorageInterface  = d.StorageInterface,
                                                                            TableOfContents   = d.TableOfContents
                                                                        })
                                                                       .ToListAsync();

    public async Task<List<MediaDto>> GetTitlesAsync() => await context.Media.OrderBy(d => d.Title)
                                                                              .Select(d => new MediaDto
                                                                               {
                                                                                   Id    = d.Id,
                                                                                   Title = d.Title
                                                                               })
                                                                              .ToListAsync();

    public async Task<MediaDto> GetAsync(ulong id) => await context.Media.Where(d => d.Id == id)
                                                                          .Select(d => new MediaDto
                                                                           {
                                                                               Id                = d.Id,
                                                                               Title             = d.Title,
                                                                               Sequence          = d.Sequence,
                                                                               LastSequence      = d.LastSequence,
                                                                               Type              = d.Type,
                                                                               WriteOffset       = d.WriteOffset,
                                                                               Sides             = d.Sides,
                                                                               Layers            = d.Layers,
                                                                               Sessions          = d.Sessions,
                                                                               Tracks            = d.Tracks,
                                                                               Sectors           = d.Sectors,
                                                                               Size              = d.Size,
                                                                               CopyProtection    = d.CopyProtection,
                                                                               PartNumber        = d.PartNumber,
                                                                               SerialNumber      = d.SerialNumber,
                                                                               Barcode           = d.Barcode,
                                                                               CatalogueNumber   = d.CatalogueNumber,
                                                                               Manufacturer      = d.Manufacturer,
                                                                               Model             = d.Model,
                                                                               Revision          = d.Revision,
                                                                               Firmware          = d.Firmware,
                                                                               PhysicalBlockSize = d.PhysicalBlockSize,
                                                                               LogicalBlockSize  = d.LogicalBlockSize,
                                                                               BlockSizes        = d.BlockSizes,
                                                                               StorageInterface  = d.StorageInterface,
                                                                               TableOfContents   = d.TableOfContents
                                                                           })
                                                                          .FirstOrDefaultAsync();

    public async Task UpdateAsync(MediaDto dto, string userId)
    {
        Media model = await context.Media.FindAsync(dto.Id);

        if(model is null) return;

        model.Title             = dto.Title;
        model.Sequence          = dto.Sequence;
        model.LastSequence      = dto.LastSequence;
        model.Type              = dto.Type;
        model.WriteOffset       = dto.WriteOffset;
        model.Sides             = dto.Sides;
        model.Layers            = dto.Layers;
        model.Sessions          = dto.Sessions;
        model.Tracks            = dto.Tracks;
        model.Sectors           = dto.Sectors;
        model.Size              = dto.Size;
        model.CopyProtection    = dto.CopyProtection;
        model.PartNumber        = dto.PartNumber;
        model.SerialNumber      = dto.SerialNumber;
        model.Barcode           = dto.Barcode;
        model.CatalogueNumber   = dto.CatalogueNumber;
        model.Manufacturer      = dto.Manufacturer;
        model.Model             = dto.Model;
        model.Revision          = dto.Revision;
        model.Firmware          = dto.Firmware;
        model.PhysicalBlockSize = dto.PhysicalBlockSize;
        model.LogicalBlockSize  = dto.LogicalBlockSize;
        model.BlockSizes        = dto.BlockSizes;
        model.StorageInterface  = dto.StorageInterface;
        model.TableOfContents   = dto.TableOfContents;
        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<ulong> CreateAsync(MediaDto dto, string userId)
    {
        var model = new Media
        {
            Title             = dto.Title,
            Sequence          = dto.Sequence,
            LastSequence      = dto.LastSequence,
            Type              = dto.Type,
            WriteOffset       = dto.WriteOffset,
            Sides             = dto.Sides,
            Layers            = dto.Layers,
            Sessions          = dto.Sessions,
            Tracks            = dto.Tracks,
            Sectors           = dto.Sectors,
            Size              = dto.Size,
            CopyProtection    = dto.CopyProtection,
            PartNumber        = dto.PartNumber,
            SerialNumber      = dto.SerialNumber,
            Barcode           = dto.Barcode,
            CatalogueNumber   = dto.CatalogueNumber,
            Manufacturer      = dto.Manufacturer,
            Model             = dto.Model,
            Revision          = dto.Revision,
            Firmware          = dto.Firmware,
            PhysicalBlockSize = dto.PhysicalBlockSize,
            LogicalBlockSize  = dto.LogicalBlockSize,
            BlockSizes        = dto.BlockSizes,
            StorageInterface  = dto.StorageInterface,
            TableOfContents   = dto.TableOfContents
        };

        await context.Media.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(ulong id, string userId)
    {
        Media item = await context.Media.FindAsync(id);

        if(item is null) return;

        context.Media.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}