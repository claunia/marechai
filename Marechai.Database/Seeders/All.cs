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
            Roles.Seed(userManager, roleManager, configuration);
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