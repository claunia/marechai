/*******************************************************************************
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
using Marechai.ApiClient.Models;
using Marechai.Helpers;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public class SoftwareAttributesService(Marechai.ApiClient.Client client)
{
    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }

    public async Task<SoftwareAttributePageDto> GetPagedAsync(int? softwareId, int? releaseId, string category,
                                                              string key, int page, int pageSize)
    {
        try
        {
            return await client.Software.Attributes.GetAsync(rc =>
            {
                rc.QueryParameters.SoftwareId = softwareId;
                rc.QueryParameters.ReleaseId  = releaseId;
                rc.QueryParameters.Category   = string.IsNullOrWhiteSpace(category) ? null : category;
                rc.QueryParameters.Key        = string.IsNullOrWhiteSpace(key) ? null : key;
                rc.QueryParameters.Page       = page;
                rc.QueryParameters.PageSize   = pageSize;
            });
        }
        catch
        {
            return new SoftwareAttributePageDto
            {
                Items      = new List<SoftwareAttributeDto>(),
                TotalCount = 0
            };
        }
    }

    public async Task<List<string>> GetDistinctCategoriesAsync()
    {
        try
        {
            return await client.Software.Attributes.DistinctCategories.GetAsync() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<List<string>> GetDistinctKeysAsync(string category = null)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            return await client.Software.Attributes.DistinctKeys.GetAsync(rc =>
            {
                rc.QueryParameters.Category = string.IsNullOrWhiteSpace(category) ? null : category;
                rc.QueryParameters.Lang     = lang;
            }) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    ///     Returns the DISTINCT non-empty <c>Value</c> strings used across software
    ///     attributes (optionally filtered by category and/or key). Used by the public
    ///     collaborative suggestion dialogs (specs / ratings): the dialog calls this with
    ///     the currently selected key to narrow the value picker corpus to values that
    ///     have actually been paired with that key (since e.g. "1 MB" only makes sense
    ///     under "RAM", not under "ESRB Rating").
    /// </summary>
    public async Task<List<string>> GetDistinctValuesAsync(string category = null, string key = null)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            return await client.Software.Attributes.DistinctValues.GetAsync(rc =>
            {
                rc.QueryParameters.Category = string.IsNullOrWhiteSpace(category) ? null : category;
                rc.QueryParameters.Key      = string.IsNullOrWhiteSpace(key)      ? null : key;
                rc.QueryParameters.Lang     = lang;
            }) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<List<SoftwareReleaseLookupDto>> LookupReleasesAsync(int softwareId)
    {
        try
        {
            return await client.Software.Attributes.LookupReleases.GetAsync(rc =>
            {
                rc.QueryParameters.SoftwareId = softwareId;
            }) ?? new List<SoftwareReleaseLookupDto>();
        }
        catch
        {
            return new List<SoftwareReleaseLookupDto>();
        }
    }

    public async Task<SoftwareAttributeDto> GetByIdAsync(long id)
    {
        try
        {
            return await client.Software.Attributes[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string error)> CreateAsync(CreateSoftwareAttributeRequest request)
    {
        try
        {
            long? id = await client.Software.Attributes.PostAsync(request);

            return (id, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateAsync(long id, UpdateSoftwareAttributeRequest request)
    {
        try
        {
            await client.Software.Attributes[id].PutAsync(request);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteAsync(long id)
    {
        try
        {
            await client.Software.Attributes[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(SplitSoftwareAttributeResultDto preview, string error)> PreviewSplitAsync(
        long id, string separator)
    {
        try
        {
            SplitSoftwareAttributeResultDto preview =
                await client.Software.Attributes[id]
                            .SplitPreview.PostAsync(new SplitSoftwareAttributeRequest { Separator = separator });

            return (preview, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(SplitSoftwareAttributeResultDto result, string error)> SplitAsync(long id, string separator)
    {
        try
        {
            SplitSoftwareAttributeResultDto result =
                await client.Software.Attributes[id]
                            .Split.PostAsync(new SplitSoftwareAttributeRequest { Separator = separator });

            return (result, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }
}
