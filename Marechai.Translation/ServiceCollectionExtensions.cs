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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marechai.Translation;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the named <c>NllbServe</c> and <c>OpenAI</c> <c>HttpClient</c>s (only when their
    ///     <c>Url</c> is configured) along with the singleton <see cref="TranslationService" />. Safe to
    ///     call from both Blazor (<c>Marechai/Startup.cs</c>) and the ASP.NET Core API
    ///     (<c>Marechai.Server/Program.cs</c>) — both paths read the same config keys.
    /// </summary>
    public static IServiceCollection AddMarechaiTranslation(this IServiceCollection services,
                                                            IConfiguration configuration)
    {
        string nllbServeUrl = configuration["NllbServe:Url"];

        if(!string.IsNullOrWhiteSpace(nllbServeUrl))
        {
            services.AddHttpClient("NllbServe", client =>
            {
                client.BaseAddress = new Uri(nllbServeUrl);
                client.Timeout     = TimeSpan.FromSeconds(120);
            });
        }

        string openAiUrl = configuration["OpenAI:Url"];

        if(!string.IsNullOrWhiteSpace(openAiUrl))
        {
            int openAiTimeoutSeconds = 600;

            if(int.TryParse(configuration["OpenAI:TimeoutSeconds"], out int parsedTimeout) && parsedTimeout > 0)
                openAiTimeoutSeconds = parsedTimeout;

            services.AddHttpClient("OpenAI", client =>
            {
                client.BaseAddress = new Uri(openAiUrl);
                client.Timeout     = TimeSpan.FromSeconds(openAiTimeoutSeconds);
            });
        }

        services.AddSingleton<TranslationService>();

        return services;
    }
}
