using System;
using System.Linq;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Marechai.Database.Seeders
{
    public static class Roles
    {
        public static void Seed(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
                                IConfiguration configuration)
        {
            var roles = configuration.GetSection("MarechaiRoles").GetChildren().Select(x => new
            {
                Name = x.GetValue<string>("Name"), Description = x.GetValue<string>("Description")
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
                                  role.Description);
            }
        }
    }
}