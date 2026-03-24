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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Globalization;
using System.Net.Http;
using MudBlazor.Services;
using Marechai.Services;
using Marechai.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Form;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Multipart;
using Microsoft.Kiota.Serialization.Text;

namespace Marechai;

public class Startup(IConfiguration configuration)
{
    readonly CultureInfo[] _supportedCultures =
    [
        new("en-US"), new("es")
    ];

    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMudServices();

        string apiUrl = Configuration.GetSection("ApiClient:Url").Value ?? "http://localhost:5023";

        services.AddSingleton(new ApiAssetUrlProvider(apiUrl));

        services.AddSingleton(_ =>
        {

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            };

            var authProvider    = new AnonymousAuthenticationProvider();
            var parseNodeRegistry = new ParseNodeFactoryRegistry();
            parseNodeRegistry.ContentTypeAssociatedFactories["application/json"] = new JsonParseNodeFactory();
            parseNodeRegistry.ContentTypeAssociatedFactories["text/plain"] = new TextParseNodeFactory();
            parseNodeRegistry.ContentTypeAssociatedFactories["application/x-www-form-urlencoded"] = new FormParseNodeFactory();
            var serializationWriterFactory = new Marechai.ApiClient.CompositeSerializationWriterFactory();
            serializationWriterFactory.AddFactory(new JsonSerializationWriterFactory());
            serializationWriterFactory.AddFactory(new MultipartSerializationWriterFactory());
            serializationWriterFactory.AddFactory(new TextSerializationWriterFactory());
            serializationWriterFactory.AddFactory(new FormSerializationWriterFactory());

            var requestAdapter = new HttpClientRequestAdapter(authProvider, parseNodeRegistry,
                                                              serializationWriterFactory, httpClient);

            return new Marechai.ApiClient.Client(requestAdapter);
        });

        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        Register.RegisterServices(services);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if(env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
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
            SupportedCultures = _supportedCultures,

            // UI strings that we have localized.
            SupportedUICultures = _supportedCultures
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Add other security headers
        app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}