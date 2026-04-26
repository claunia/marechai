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

using Marechai.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Marechai.Services;

public static class Register
{
    internal static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<StringLocalizer<NavMenu>>();

        services.AddScoped<NewsService>();
        services.AddScoped<CompaniesService>();
        services.AddScoped<CompanyLogosService>();
        services.AddScoped<ComputersService>();
        services.AddScoped<ConsolesService>();
        services.AddScoped<MachinesService>();
        services.AddScoped<MachineFamiliesService>();
        services.AddScoped<MachinePhotosService>();
        services.AddScoped<GpusService>();
        services.AddScoped<InstructionSetsService>();
        services.AddScoped<InstructionSetExtensionsService>();
        services.AddScoped<ProcessorsService>();
        services.AddScoped<ResolutionsService>();
        services.AddScoped<ScreensService>();
        services.AddScoped<SoundSynthsService>();
        services.AddScoped<BooksService>();
        services.AddScoped<DocumentsService>();
        services.AddScoped<MagazinesService>();
        services.AddScoped<PeopleService>();
        services.AddScoped<PeopleByCompanyService>();
        services.AddScoped<SoftwareService>();
        services.AddScoped<SoftwareFamiliesService>();
        services.AddScoped<SoftwarePlatformsService>();
        services.AddScoped<SoftwareVersionsService>();
        services.AddScoped<SoftwareReleasesService>();
        services.AddScoped<AuthService>();
        services.AddScoped<UsersService>();
    }
}