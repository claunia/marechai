/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Company.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of a company.
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

namespace Cicm.Database.Schemas
{
    /// <summary>Company</summary>
    public class Company
    {
        /// <summary>Address</summary>
        public string Address;
        /// <summary>City</summary>
        public string City;
        /// <summary>Country</summary>
        public Iso3166 Country;
        /// <summary>Facebook account</summary>
        public string Facebook;
        /// <summary>Founding date</summary>
        public DateTime Founded;
        /// <summary>ID</summary>
        public int Id;
        /// <summary>Name</summary>
        public string Name;
        /// <summary>Postal code</summary>
        public string PostalCode;
        /// <summary>Province</summary>
        public string Province;
        /// <summary>Sold date</summary>
        public DateTime Sold;
        /// <summary>Company it was sold to</summary>
        public Company SoldTo;
        /// <summary>Company status</summary>
        public CompanyStatus Status;
        /// <summary>Twitter account</summary>
        public string Twitter;
        /// <summary>Website</summary>
        public string Website;
    }
}