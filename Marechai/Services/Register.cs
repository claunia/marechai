/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : Register.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Register services for injection
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

using Marechai.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Marechai.Services
{
    public static class Register
    {
        internal static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<StringLocalizer<NavMenu>>();

            // TODO: Use reflection
            services.AddScoped<NewsService>();
            services.AddScoped<CompaniesService>();
            services.AddScoped<ComputersService>();
            services.AddScoped<ConsolesService>();
            services.AddScoped<MachinesService>();
            services.AddScoped<AdminService>();
            services.AddScoped<BrowserTestsService>();
            services.AddScoped<DocumentCompaniesService>();
            services.AddScoped<DocumentPeopleService>();
            services.AddScoped<GpusService>();
            services.AddScoped<InstructionSetExtensionsService>();
            services.AddScoped<InstructionSetsService>();
            services.AddScoped<LicensesService>();
            services.AddScoped<MachineFamiliesService>();
            services.AddScoped<PeopleService>();
            services.AddScoped<ProcessorsService>();
            services.AddScoped<ScreensService>();
            services.AddScoped<SoundSynthsService>();
            services.AddScoped<Iso31661NumericService>();
            services.AddScoped<ResolutionsService>();
            services.AddScoped<CompanyLogosService>();
            services.AddScoped<GpusByMachineService>();
            services.AddScoped<SoundSynthsByMachineService>();
            services.AddScoped<ProcessorsByMachineService>();
            services.AddScoped<MemoriesByMachineService>();
            services.AddScoped<StorageByMachineService>();
            services.AddScoped<ScreensByMachineService>();
            services.AddScoped<ResolutionsService>();
            services.AddScoped<ResolutionsByScreenService>();
            services.AddScoped<ResolutionsByGpuService>();
        }
    }
}