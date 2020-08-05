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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class CompanyLogosService
    {
        readonly MarechaiContext     _context;
        readonly IWebHostEnvironment _host;
        readonly string              _webRootPath;

        public CompanyLogosService(MarechaiContext context, IWebHostEnvironment host)
        {
            _context     = context;
            _host        = host;
            _webRootPath = host.WebRootPath;
        }

        public async Task<List<CompanyLogo>> GetByCompany(int companyId) =>
            await _context.CompanyLogos.Where(l => l.CompanyId == companyId).OrderBy(l => l.Year).ToListAsync();

        public async Task DeleteAsync(int id, string userId)
        {
            CompanyLogo logo = await _context.CompanyLogos.Where(l => l.Id == id).FirstOrDefaultAsync();

            if(logo is null)
                return;

            _context.CompanyLogos.Remove(logo);
            await _context.SaveChangesWithUserAsync(userId);

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos", logo.Guid + ".svg")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos", logo.Guid + ".svg"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/webp/1x", logo.Guid + ".webp")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/webp/1x", logo.Guid + ".webp"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/webp/2x", logo.Guid + ".webp")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/webp/2x", logo.Guid + ".webp"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/webp/3x", logo.Guid + ".webp")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/webp/3x", logo.Guid + ".webp"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/png/1x", logo.Guid + ".png")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/png/1x", logo.Guid + ".png"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/png/2x", logo.Guid + ".png")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/png/2x", logo.Guid + ".png"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/png/3x", logo.Guid + ".png")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/png/3x", logo.Guid + ".png"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/1x", logo.Guid + ".webp")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/1x", logo.Guid + ".webp"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/2x", logo.Guid + ".webp")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/2x", logo.Guid + ".webp"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/3x", logo.Guid + ".webp")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/3x", logo.Guid + ".webp"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/png/1x", logo.Guid + ".png")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/png/1x", logo.Guid + ".png"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/png/2x", logo.Guid + ".png")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/png/2x", logo.Guid + ".png"));

            if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/png/3x", logo.Guid + ".png")))
                File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/png/3x", logo.Guid + ".png"));
        }

        public async Task ChangeYearAsync(int id, int? year, string userId)
        {
            CompanyLogo logo = await _context.CompanyLogos.Where(l => l.Id == id).FirstOrDefaultAsync();

            if(logo is null)
                return;

            logo.Year = year;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(int companyId, Guid guid, int? year, string userId)
        {
            var logo = new CompanyLogo
            {
                Guid      = guid,
                Year      = year,
                CompanyId = companyId
            };

            await _context.CompanyLogos.AddAsync(logo);
            await _context.SaveChangesWithUserAsync(userId);

            return logo.Id;
        }
    }
}