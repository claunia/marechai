using System;
using System.Collections.Generic;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Seeders
{
    public static class Users
    {
        public static void Seed(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            ApplicationRole uberAdminRole = roleManager.FindByNameAsync("UberAdmin").Result;

            if(uberAdminRole is null)
            {
                Console.WriteLine("Cannot find UberAdmin role, is database properly seeded?");

                return;
            }

            IList<ApplicationUser> uberAdmins = userManager.GetUsersInRoleAsync("UberAdmin").Result;

            if(uberAdmins.Count > 1)
            {
                Console.WriteLine("Too many uberadmins, only one can exist");

                foreach(ApplicationUser user in uberAdmins)
                {
                    Console.WriteLine("Removing uberadmin permissions from user {0}", user.UserName);

                    userManager.RemoveFromRoleAsync(user, "UberAdmin");
                }
            }

            if(uberAdmins.Count == 1)
                return;

            ApplicationUser uberAdmin = userManager.FindByEmailAsync("claunia@claunia.com").Result ??
                                        userManager.FindByNameAsync("claunia").Result;

            if(uberAdmin is null)
            {
                uberAdmin = new ApplicationUser
                {
                    UserName = "claunia", Email = "claunia@claunia.com", EmailConfirmed = true
                };

                byte[] newPass = new byte[8];
                new Random().NextBytes(newPass);
                string newPassString = Convert.ToBase64String(newPass);

                IdentityResult result = userManager.CreateAsync(uberAdmin, newPassString).Result;

                if(result.Succeeded)
                {
                    userManager.AddToRoleAsync(uberAdmin, "UberAdmin").Wait();
                    Console.WriteLine("Created new claunia uberadmin with password {0}.", newPassString);
                }
                else
                {
                    Console.WriteLine("Could not create new uberadmin.");
                }

                return;
            }

            Console.WriteLine("Giving uberadmin permissions to user claunia");
            userManager.AddToRoleAsync(uberAdmin, "UberAdmin").Wait();
        }
    }
}