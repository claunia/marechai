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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class SoftwareVariantsService
    {
        readonly MarechaiContext _context;

        public SoftwareVariantsService(MarechaiContext context) => _context = context;

        public async Task<List<SoftwareVariantViewModel>> GetAsync() => await _context.
                                                                              SoftwareVariants.
                                                                              OrderBy(b => b.SoftwareVersion.Family.
                                                                                  Name).
                                                                              ThenBy(b => b.SoftwareVersion.Version).
                                                                              ThenBy(b => b.Name).
                                                                              ThenBy(b => b.Version).
                                                                              ThenBy(b => b.Introduced).
                                                                              Select(b => new SoftwareVariantViewModel
                                                                              {
                                                                                  Id         = b.Id,
                                                                                  Name       = b.Name,
                                                                                  Version    = b.Version,
                                                                                  Introduced = b.Introduced,
                                                                                  ParentId   = b.ParentId,
                                                                                  Parent =
                                                                                      b.Parent.Name ?? b.Parent.Version,
                                                                                  SoftwareVersionId =
                                                                                      b.SoftwareVersionId,
                                                                                  SoftwareVersion =
                                                                                      b.SoftwareVersion.Name ??
                                                                                      b.SoftwareVersion.Version,
                                                                                  MinimumMemory = b.MinimumMemory,
                                                                                  RecommendedMemory =
                                                                                      b.RecommendedMemory,
                                                                                  RequiredStorage  = b.RequiredStorage,
                                                                                  PartNumber       = b.PartNumber,
                                                                                  SerialNumber     = b.SerialNumber,
                                                                                  ProductCode      = b.ProductCode,
                                                                                  CatalogueNumber  = b.CatalogueNumber,
                                                                                  DistributionMode = b.DistributionMode
                                                                              }).ToListAsync();

        public async Task<SoftwareVariantViewModel> GetAsync(ulong id) =>
            await _context.SoftwareVariants.Where(b => b.Id == id).Select(b => new SoftwareVariantViewModel
            {
                Id                = b.Id,
                Name              = b.Name,
                Version           = b.Version,
                Introduced        = b.Introduced,
                ParentId          = b.ParentId,
                Parent            = b.Parent.Name ?? b.Parent.Version,
                SoftwareVersionId = b.SoftwareVersionId,
                SoftwareVersion   = b.SoftwareVersion.Name ?? b.SoftwareVersion.Version,
                MinimumMemory     = b.MinimumMemory,
                RecommendedMemory = b.RecommendedMemory,
                RequiredStorage   = b.RequiredStorage,
                PartNumber        = b.PartNumber,
                SerialNumber      = b.SerialNumber,
                ProductCode       = b.ProductCode,
                CatalogueNumber   = b.CatalogueNumber,
                DistributionMode  = b.DistributionMode
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(SoftwareVariantViewModel viewModel, string userId)
        {
            SoftwareVariant model = await _context.SoftwareVariants.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name              = viewModel.Name;
            model.Version           = viewModel.Version;
            model.Introduced        = viewModel.Introduced;
            model.ParentId          = viewModel.ParentId;
            model.SoftwareVersionId = viewModel.SoftwareVersionId;
            model.MinimumMemory     = viewModel.MinimumMemory;
            model.RecommendedMemory = viewModel.RecommendedMemory;
            model.RequiredStorage   = viewModel.RequiredStorage;
            model.PartNumber        = viewModel.PartNumber;
            model.SerialNumber      = viewModel.SerialNumber;
            model.ProductCode       = viewModel.ProductCode;
            model.CatalogueNumber   = viewModel.CatalogueNumber;
            model.DistributionMode  = viewModel.DistributionMode;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<ulong> CreateAsync(SoftwareVariantViewModel viewModel, string userId)
        {
            var model = new SoftwareVariant
            {
                Name              = viewModel.Name,
                Version           = viewModel.Version,
                Introduced        = viewModel.Introduced,
                ParentId          = viewModel.ParentId,
                SoftwareVersionId = viewModel.SoftwareVersionId,
                MinimumMemory     = viewModel.MinimumMemory,
                RecommendedMemory = viewModel.RecommendedMemory,
                RequiredStorage   = viewModel.RequiredStorage,
                PartNumber        = viewModel.PartNumber,
                SerialNumber      = viewModel.SerialNumber,
                ProductCode       = viewModel.ProductCode,
                CatalogueNumber   = viewModel.CatalogueNumber,
                DistributionMode  = viewModel.DistributionMode
            };

            await _context.SoftwareVariants.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(ulong id, string userId)
        {
            SoftwareVariant item = await _context.SoftwareVariants.FindAsync(id);

            if(item is null)
                return;

            _context.SoftwareVariants.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}