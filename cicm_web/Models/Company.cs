/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Company.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Company model
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Cicm.Database.Schemas;
using Markdig;

namespace cicm_web.Models
{
    public class CompanyWithItems
    {
        public string                        Address;
        public string                        City;
        public ComputerMini[]                Computers;
        public ConsoleMini[]                 Consoles;
        public Iso3166                       Country;
        public string                        Facebook;
        public DateTime                      Founded;
        public int                           Id;
        public CompanyLogo                   LastLogo;
        public CompanyLogo[]                 Logos;
        public string                        Name;
        public string                        PostalCode;
        public string                        Province;
        public DateTime                      Sold;
        public Cicm.Database.Schemas.Company SoldTo;
        public CompanyStatus                 Status;
        public string                        Twitter;
        public string                        Website;
        public string Description;

        public static CompanyWithItems GetItem(int id)
        {
            Cicm.Database.Schemas.Company dbItem = Program.Database?.Operations.GetCompany(id);
            MarkdownPipeline pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            return dbItem == null
                       ? null
                       : new CompanyWithItems
                       {
                           Name       = dbItem.Name,
                           Id         = dbItem.Id,
                           Computers  = ComputerMini.GetItemsWithCompany(id, dbItem.Name),
                           Consoles   = ConsoleMini.GetItemsWithCompany(id, dbItem.Name),
                           Address    = dbItem.Address,
                           City       = dbItem.City,
                           Country    = dbItem.Country,
                           Facebook   = dbItem.Facebook,
                           Founded    = dbItem.Founded,
                           PostalCode = dbItem.PostalCode,
                           Province   = dbItem.Province,
                           Sold       = dbItem.Sold,
                           SoldTo     = dbItem.SoldTo,
                           Status     = dbItem.Status,
                           Twitter    = dbItem.Twitter,
                           Website    = dbItem.Website,
                           Logos      = dbItem.Logos,
                           LastLogo   = dbItem.LastLogo,
                           Description = dbItem.Description == null ? null : Markdig.Markdown.ToHtml(dbItem.Description, pipeline)
                       };
        }

        public static CompanyWithItems[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Company> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetCompanies(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;
            MarkdownPipeline pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            return dbItems.Select(t => new CompanyWithItems
            {
                Id         = t.Id,
                Name       = t.Name,
                Computers  = ComputerMini.GetItemsWithCompany(t.Id, t.Name),
                Consoles   = ConsoleMini.GetItemsWithCompany(t.Id, t.Name),
                Address    = t.Address,
                City       = t.City,
                Country    = t.Country,
                Facebook   = t.Facebook,
                Founded    = t.Founded,
                PostalCode = t.PostalCode,
                Province   = t.Province,
                Sold       = t.Sold,
                SoldTo     = t.SoldTo,
                Status     = t.Status,
                Twitter    = t.Twitter,
                Website    = t.Website,
                Logos      = t.Logos,
                LastLogo   = t.LastLogo,
                Description = t.Description == null ? null : Markdig.Markdown.ToHtml(t.Description, pipeline)
            }).OrderBy(t => t.Name).ToArray();
        }

        public static CompanyWithItems[] GetItemsStartingWithLetter(char letter)
        {
            List<Cicm.Database.Schemas.Company> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetCompanies(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;
            MarkdownPipeline pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            return dbItems
                  .Where(t => t.Name.StartsWith(new string(letter, 1), StringComparison.InvariantCultureIgnoreCase))
                  .Select(t => new CompanyWithItems
                   {
                       Id         = t.Id,
                       Name       = t.Name,
                       Computers  = ComputerMini.GetItemsWithCompany(t.Id, t.Name),
                       Consoles   = ConsoleMini.GetItemsWithCompany(t.Id, t.Name),
                       Address    = t.Address,
                       City       = t.City,
                       Country    = t.Country,
                       Facebook   = t.Facebook,
                       Founded    = t.Founded,
                       PostalCode = t.PostalCode,
                       Province   = t.Province,
                       Sold       = t.Sold,
                       SoldTo     = t.SoldTo,
                       Status     = t.Status,
                       Twitter    = t.Twitter,
                       Website    = t.Website,
                       Logos      = t.Logos,
                       LastLogo   = t.LastLogo,
                       Description = t.Description == null ? null : Markdig.Markdown.ToHtml(t.Description, pipeline)
                   }).OrderBy(t => t.Name).ToArray();
        }
    }

    public class Company
    {
        public int         Id;
        public CompanyLogo LastLogo;
        public string      Name;

        public static Company GetItem(int id)
        {
            Cicm.Database.Schemas.Company dbItem = Program.Database?.Operations.GetCompany(id);

            return dbItem == null ? null : new Company {Name = dbItem.Name, Id = dbItem.Id, LastLogo = dbItem.LastLogo};
        }

        public static Company[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Company> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetCompanies(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.Select(t => new Company {Id = t.Id, Name = t.Name, LastLogo = t.LastLogo})
                          .OrderBy(t => t.Name).ToArray();
        }

        public static Company[] GetItemsStartingWithLetter(char letter)
        {
            List<Cicm.Database.Schemas.Company> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetCompanies(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems
                  .Where(t => t.Name.StartsWith(new string(letter, 1), StringComparison.InvariantCultureIgnoreCase))
                  .Select(t => new Company {Id = t.Id, Name = t.Name, LastLogo = t.LastLogo}).OrderBy(t => t.Name)
                  .ToArray();
        }
    }
}