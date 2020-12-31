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
using System.Globalization;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Marechai.Areas.Identity;
using Marechai.Database.Models;
using Marechai.Database.Seeders;
using Marechai.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tewr.Blazor.FileReader;

namespace Marechai
{
    public class Startup
    {
        readonly CultureInfo[] supportedCultures =
        {
            new("en-US"), new("es")
        };

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlazorise(options => options.ChangeTextOnKeyPress = true).AddBootstrapProviders().
                     AddFontAwesomeIcons();

            services.AddDbContext<MarechaiContext>(options => options.UseLazyLoadingProxies().
                                                                      UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                                                                               new MariaDbServerVersion(new Version(10,
                                                                                   5, 0)),
                                                                               b => b.UseMicrosoftJson()));

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).
                     AddRoles<ApplicationRole>().AddEntityFrameworkStores<MarechaiContext>();

            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.
                AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>
                >();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddFileReaderService();

            Register.RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MarechaiContext context,
                              UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),

                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,

                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.ApplicationServices.UseBootstrapProviders().UseFontAwesomeIcons();

            app.UseAuthentication();
            app.UseAuthorization();

            All.Seed(context, userManager, roleManager, Configuration);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}