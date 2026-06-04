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
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using ChangeYearBody =
    Marechai.ApiClient.Companies.Logos.ChangeYear.Item.ChangeYearItemRequestBuilder.ChangeYearPutRequestBody;

namespace Marechai.Services;

public class CompanyLogosService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter)
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
            return (false, ExtractDetail(ex));
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
            return (false, ExtractDetail(ex));
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
            var body = new MultipartBody();
            body.AddOrReplacePart("file", "image/svg+xml", new MemoryStream(svgBytes), "logo.svg");
            body.AddOrReplacePart("companyId", "text/plain", companyId.ToString());

            if(year.HasValue)
                body.AddOrReplacePart("year", "text/plain", year.Value.ToString());

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST, "{+baseurl}/companies/logos/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            CompanyLogoDto result = await requestAdapter.SendAsync(requestInfo,
                                                                   CompanyLogoDto.CreateFromDiscriminatorValue,
                                                                   errorMapping);

            return (result, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    static string ExtractDetail(ApiException ex)
    {
        // Kiota maps server error responses to a typed ProblemDetails (which inherits from
        // ApiException). The base Exception.Message just returns "Exception of type 'X' was
        // thrown." — the real, user-facing text lives on Detail / Title. Surface those when
        // present, falling back to Message only if the server gave us nothing useful.
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }
}
