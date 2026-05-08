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
using System.Net.Http.Headers;
using MudBlazor.Services;
using Marechai.Email;
using Marechai.Services;
using Marechai.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kiota.Abstractions;
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
        new("en-US"), new("es"), new("fr"), new("it"), new("de")
    ];

    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMudServices();

        services.AddMarechaiEmail(Configuration);

        string apiUrl = Configuration.GetSection("ApiClient:Url").Value ?? "http://localhost:5023";

        services.AddSingleton(new ApiAssetUrlProvider(apiUrl));

        services.AddScoped<TokenProvider>();

        services.AddScoped<JwtAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

        services.AddScoped<IRequestAdapter>(sp =>
        {
            TokenProvider tokenProvider = sp.GetRequiredService<TokenProvider>();

            var authHandler = new HttpAuthHandler(tokenProvider)
            {
                InnerHandler = new HttpClientHandler()
            };

            var httpClient = new HttpClient(authHandler)
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

            return new HttpClientRequestAdapter(authProvider, parseNodeRegistry,
                                                serializationWriterFactory, httpClient);
        });

        services.AddScoped(sp => new Marechai.ApiClient.Client(sp.GetRequiredService<IRequestAdapter>()));

        services.AddHttpClient("Plausible", client =>
        {
            client.BaseAddress = new Uri("https://plausible.claunia.com");
        });

        string nllbServeUrl = Configuration["NllbServe:Url"];

        if(!string.IsNullOrWhiteSpace(nllbServeUrl))
        {
            services.AddHttpClient("NllbServe", client =>
            {
                client.BaseAddress = new Uri(nllbServeUrl);
                client.Timeout     = TimeSpan.FromSeconds(120);
            });
        }

        string openAiUrl = Configuration["OpenAI:Url"];

        if(!string.IsNullOrWhiteSpace(openAiUrl))
        {
            int openAiTimeoutSeconds = 600;

            if(int.TryParse(Configuration["OpenAI:TimeoutSeconds"], out int parsedTimeout) && parsedTimeout > 0)
                openAiTimeoutSeconds = parsedTimeout;

            services.AddHttpClient("OpenAI", client =>
            {
                client.BaseAddress = new Uri(openAiUrl);
                client.Timeout     = TimeSpan.FromSeconds(openAiTimeoutSeconds);
            });
        }

        services.AddAuthorizationCore();
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
            endpoints.MapGet("/js/pa.js", async context =>
            {
                var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                var client        = clientFactory.CreateClient("Plausible");

                HttpResponseMessage response;

                try
                {
                    response = await client.GetAsync("/js/pa-v714IfTDuCkgEe0Q7qxoq.js");
                }
                catch
                {
                    context.Response.StatusCode = 502;

                    return;
                }

                if(!response.IsSuccessStatusCode)
                {
                    context.Response.StatusCode = (int)response.StatusCode;

                    return;
                }

                context.Response.ContentType              = "application/javascript";
                context.Response.Headers["Cache-Control"] = "public, max-age=86400";

                await response.Content.CopyToAsync(context.Response.Body);
            });

            endpoints.MapPost("/api/event", async context =>
            {
                var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                var client        = clientFactory.CreateClient("Plausible");

                var proxyRequest = new HttpRequestMessage(HttpMethod.Post, "/api/event");
                proxyRequest.Content = new StreamContent(context.Request.Body);

                if(context.Request.ContentType != null)
                {
                    proxyRequest.Content.Headers.ContentType =
                        MediaTypeHeaderValue.Parse(context.Request.ContentType);
                }

                if(context.Request.Headers.TryGetValue("User-Agent", out var ua))
                    proxyRequest.Headers.TryAddWithoutValidation("User-Agent", ua.ToString());

                proxyRequest.Headers.TryAddWithoutValidation("X-Forwarded-For",
                    context.Request.Headers.TryGetValue("X-Forwarded-For", out var xff)
                        ? xff.ToString()
                        : context.Connection.RemoteIpAddress?.ToString());

                HttpResponseMessage response;

                try
                {
                    response = await client.SendAsync(proxyRequest);
                }
                catch
                {
                    context.Response.StatusCode = 502;

                    return;
                }

                context.Response.StatusCode = (int)response.StatusCode;

                await response.Content.CopyToAsync(context.Response.Body);
            });

            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}