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
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Companies.Logos.Upload;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;
using ChangeYearBody =
    Marechai.ApiClient.Companies.Logos.ChangeYear.Item.ChangeYearItemRequestBuilder.ChangeYearPutRequestBody;

namespace Marechai.Services;

public class CompanyLogosService(Marechai.ApiClient.Client client)
{
    public async Task<List<CompanyLogoDto>> GetByCompany(int companyId)
    {
        try
        {
            List<CompanyLogoDto> logos = await client.Companies[companyId].Logos.GetAsync();

            return logos ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> DeleteAsync(int logoId)
    {
        try
        {
            await client.Companies.Logos[logoId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> ChangeYearAsync(int logoId, int? year)
    {
        try
        {
            var body = new ChangeYearBody
            {
                Integer = year
            };

            await client.Companies.Logos.ChangeYear[logoId].PutAsync(body);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(CompanyLogoDto logo, string error)> UploadAsync(int companyId, byte[] svgBytes, int? year)
    {
        try
        {
            var body = new UploadPostRequestBody
            {
                CompanyId = companyId,
                File      = svgBytes,
                Year      = year
            };

            CompanyLogoDto result = await client.Companies.Logos.Upload.PostAsync(body);

            return (result, null);
        }
        catch(ApiException ex)
        {
            return (null, ex.Message);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }
}
