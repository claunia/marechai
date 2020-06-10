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
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Marechai.Database.Seeders
{
    public static class All
    {
        public static void Seed(MarechaiContext context, UserManager<ApplicationUser> userManager,
                                RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            DateTime start, end;

            start = DateTime.Now;
            Console.WriteLine("\u001b[31;1mImporting ISO-639 codes...\u001b[0m");

            try
            {
                Iso639.Seed(context);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception {0} importing ISO-639 codes, saving changes and continuing...", e);
            }

            context.SaveChanges();

            end = DateTime.Now;

            Console.WriteLine("\u001b[31;1mTook \u001b[32;1m{0} seconds\u001b[31;1m...\u001b[0m",
                              (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("\u001b[31;1mImporting ISO-4217 codes...\u001b[0m");

            try
            {
                Iso4217.Seed(context);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception {0} importing ISO-4217 codes, saving changes and continuing...", e);
            }

            context.SaveChanges();

            end = DateTime.Now;

            Console.WriteLine("\u001b[31;1mTook \u001b[32;1m{0} seconds\u001b[31;1m...\u001b[0m",
                              (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("\u001b[31;1mSeeding document roles...\u001b[0m");
            DocumentRoles.Seed(context);
            end = DateTime.Now;

            Console.WriteLine("\u001b[31;1mTook \u001b[32;1m{0} seconds\u001b[31;1m...\u001b[0m",
                              (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("\u001b[31;1mSeeding licenses...\u001b[0m");
            License.Seed(context);
            end = DateTime.Now;

            Console.WriteLine("\u001b[31;1mTook \u001b[32;1m{0} seconds\u001b[31;1m...\u001b[0m",
                              (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("\u001b[31;1mSeeding application roles...\u001b[0m");
            Roles.Seed(roleManager, configuration);
            end = DateTime.Now;

            Console.WriteLine("\u001b[31;1mTook \u001b[32;1m{0} seconds\u001b[31;1m...\u001b[0m",
                              (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("\u001b[31;1mSeeding application users...\u001b[0m");
            Users.Seed(userManager, roleManager);
            end = DateTime.Now;

            Console.WriteLine("\u001b[31;1mTook \u001b[32;1m{0} seconds\u001b[31;1m...\u001b[0m",
                              (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("\u001b[31;1mSaving changes...\u001b[0m");
            context.SaveChanges();
            end = DateTime.Now;

            Console.WriteLine("\u001b[31;1mTook \u001b[32;1m{0} seconds\u001b[31;1m...\u001b[0m",
                              (end - start).TotalSeconds);
        }
    }
}