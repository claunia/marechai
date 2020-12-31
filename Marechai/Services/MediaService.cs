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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class MediaService
    {
        readonly MarechaiContext _context;

        public MediaService(MarechaiContext context) => _context = context;

        public async Task<List<MediaViewModel>> GetAsync() => await _context.
                                                                    Media.OrderBy(d => d.Title).
                                                                    Select(d => new MediaViewModel
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
                                                                    }).ToListAsync();

        public async Task<List<MediaViewModel>> GetTitlesAsync() => await _context.
                                                                          Media.OrderBy(d => d.Title).
                                                                          Select(d => new MediaViewModel
                                                                          {
                                                                              Id    = d.Id,
                                                                              Title = d.Title
                                                                          }).ToListAsync();

        public async Task<MediaViewModel> GetAsync(ulong id) => await _context.Media.Where(d => d.Id == id).
                                                                               Select(d => new MediaViewModel
                                                                               {
                                                                                   Id              = d.Id,
                                                                                   Title           = d.Title,
                                                                                   Sequence        = d.Sequence,
                                                                                   LastSequence    = d.LastSequence,
                                                                                   Type            = d.Type,
                                                                                   WriteOffset     = d.WriteOffset,
                                                                                   Sides           = d.Sides,
                                                                                   Layers          = d.Layers,
                                                                                   Sessions        = d.Sessions,
                                                                                   Tracks          = d.Tracks,
                                                                                   Sectors         = d.Sectors,
                                                                                   Size            = d.Size,
                                                                                   CopyProtection  = d.CopyProtection,
                                                                                   PartNumber      = d.PartNumber,
                                                                                   SerialNumber    = d.SerialNumber,
                                                                                   Barcode         = d.Barcode,
                                                                                   CatalogueNumber = d.CatalogueNumber,
                                                                                   Manufacturer    = d.Manufacturer,
                                                                                   Model           = d.Model,
                                                                                   Revision        = d.Revision,
                                                                                   Firmware        = d.Firmware,
                                                                                   PhysicalBlockSize =
                                                                                       d.PhysicalBlockSize,
                                                                                   LogicalBlockSize =
                                                                                       d.LogicalBlockSize,
                                                                                   BlockSizes = d.BlockSizes,
                                                                                   StorageInterface =
                                                                                       d.StorageInterface,
                                                                                   TableOfContents = d.TableOfContents
                                                                               }).FirstOrDefaultAsync();

        public async Task UpdateAsync(MediaViewModel viewModel, string userId)
        {
            Media model = await _context.Media.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Title             = viewModel.Title;
            model.Sequence          = viewModel.Sequence;
            model.LastSequence      = viewModel.LastSequence;
            model.Type              = viewModel.Type;
            model.WriteOffset       = viewModel.WriteOffset;
            model.Sides             = viewModel.Sides;
            model.Layers            = viewModel.Layers;
            model.Sessions          = viewModel.Sessions;
            model.Tracks            = viewModel.Tracks;
            model.Sectors           = viewModel.Sectors;
            model.Size              = viewModel.Size;
            model.CopyProtection    = viewModel.CopyProtection;
            model.PartNumber        = viewModel.PartNumber;
            model.SerialNumber      = viewModel.SerialNumber;
            model.Barcode           = viewModel.Barcode;
            model.CatalogueNumber   = viewModel.CatalogueNumber;
            model.Manufacturer      = viewModel.Manufacturer;
            model.Model             = viewModel.Model;
            model.Revision          = viewModel.Revision;
            model.Firmware          = viewModel.Firmware;
            model.PhysicalBlockSize = viewModel.PhysicalBlockSize;
            model.LogicalBlockSize  = viewModel.LogicalBlockSize;
            model.BlockSizes        = viewModel.BlockSizes;
            model.StorageInterface  = viewModel.StorageInterface;
            model.TableOfContents   = viewModel.TableOfContents;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<ulong> CreateAsync(MediaViewModel viewModel, string userId)
        {
            var model = new Media
            {
                Title             = viewModel.Title,
                Sequence          = viewModel.Sequence,
                LastSequence      = viewModel.LastSequence,
                Type              = viewModel.Type,
                WriteOffset       = viewModel.WriteOffset,
                Sides             = viewModel.Sides,
                Layers            = viewModel.Layers,
                Sessions          = viewModel.Sessions,
                Tracks            = viewModel.Tracks,
                Sectors           = viewModel.Sectors,
                Size              = viewModel.Size,
                CopyProtection    = viewModel.CopyProtection,
                PartNumber        = viewModel.PartNumber,
                SerialNumber      = viewModel.SerialNumber,
                Barcode           = viewModel.Barcode,
                CatalogueNumber   = viewModel.CatalogueNumber,
                Manufacturer      = viewModel.Manufacturer,
                Model             = viewModel.Model,
                Revision          = viewModel.Revision,
                Firmware          = viewModel.Firmware,
                PhysicalBlockSize = viewModel.PhysicalBlockSize,
                LogicalBlockSize  = viewModel.LogicalBlockSize,
                BlockSizes        = viewModel.BlockSizes,
                StorageInterface  = viewModel.StorageInterface,
                TableOfContents   = viewModel.TableOfContents
            };

            await _context.Media.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(ulong id, string userId)
        {
            Media item = await _context.Media.FindAsync(id);

            if(item is null)
                return;

            _context.Media.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}