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

using System;
using System.Linq;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Marechai.Database.Seeders
{
    public static class Roles
    {
        public static void Seed(RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            var roles = configuration.GetSection("MarechaiRoles").GetChildren().Select(x => new
            {
                Name        = x.GetValue<string>("Name"),
                Description = x.GetValue<string>("Description")
            }).ToList();

            if(roles.Count == 0)
                return;

            foreach(var role in roles)
            {
                ApplicationRole existingRole = roleManager.FindByNameAsync(role.Name).Result;

                if(existingRole != null)
                {
                    if(existingRole.Description != role.Description)
                    {
                        Console.WriteLine("Updating description for role {0}", role.Name);
                        existingRole.Description = role.Description;
                    }

                    continue;
                }

                var newRole = new ApplicationRole(role.Name, role.Description);

                IdentityResult result = roleManager.CreateAsync(newRole).Result;

                Console.WriteLine(result.Succeeded ? "New role {0} added successfully" : "Failed to add new role {0}",
                                  role.Name);
            }
        }
    }
}