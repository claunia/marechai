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
using Marechai.ApiClient;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public sealed class CollectionService(Client client, ILogger<CollectionService> logger)
{
    // ── Public: view any user's collection ──

    public async Task<UserCollectionSummaryDto?> GetCollectionSummaryAsync(string username)
    {
        try
        {
            return await client.Profile[username].Collection.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading collection summary for {Username}", username);

            return null;
        }
    }

    public async Task<List<CollectedBookDto>?> GetCollectedBooksAsync(string username)
    {
        try
        {
            return await client.Profile[username].Collection.Books.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading collected books for {Username}", username);

            return null;
        }
    }

    public async Task<List<CollectedDocumentDto>?> GetCollectedDocumentsAsync(string username)
    {
        try
        {
            return await client.Profile[username].Collection.Documents.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading collected documents for {Username}", username);

            return null;
        }
    }

    public async Task<List<CollectedMachineDto>?> GetCollectedMachinesAsync(string username)
    {
        try
        {
            return await client.Profile[username].Collection.Machines.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading collected machines for {Username}", username);

            return null;
        }
    }

    public async Task<List<CollectedSoftwareReleaseDto>?> GetCollectedSoftwareReleasesAsync(string username)
    {
        try
        {
            return await client.Profile[username].Collection.SoftwareReleases.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading collected software releases for {Username}", username);

            return null;
        }
    }

    // ── Authenticated: manage own collection ──

    public async Task<bool> IsBookCollectedAsync(long bookId)
    {
        try
        {
            await client.Auth.Me.Collection.Books[bookId].GetAsync();

            return true;
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 404)
        {
            return false;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error checking if book {BookId} is collected", bookId);

            return false;
        }
    }

    public async Task<(bool success, string? error)> AddBookToCollectionAsync(long bookId)
    {
        try
        {
            await client.Auth.Me.Collection.Books[bookId].PostAsync();

            return (true, null);
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 409)
        {
            return (true, null); // Already collected, treat as success
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error adding book {BookId} to collection", bookId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error adding book {BookId} to collection", bookId);

            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> RemoveBookFromCollectionAsync(long bookId)
    {
        try
        {
            await client.Auth.Me.Collection.Books[bookId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error removing book {BookId} from collection", bookId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error removing book {BookId} from collection", bookId);

            return (false, ex.Message);
        }
    }

    public async Task<bool> IsDocumentCollectedAsync(long documentId)
    {
        try
        {
            await client.Auth.Me.Collection.Documents[documentId].GetAsync();

            return true;
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 404)
        {
            return false;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error checking if document {DocumentId} is collected", documentId);

            return false;
        }
    }

    public async Task<(bool success, string? error)> AddDocumentToCollectionAsync(long documentId)
    {
        try
        {
            await client.Auth.Me.Collection.Documents[documentId].PostAsync();

            return (true, null);
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 409)
        {
            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error adding document {DocumentId} to collection", documentId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error adding document {DocumentId} to collection", documentId);

            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> RemoveDocumentFromCollectionAsync(long documentId)
    {
        try
        {
            await client.Auth.Me.Collection.Documents[documentId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error removing document {DocumentId} from collection", documentId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error removing document {DocumentId} from collection", documentId);

            return (false, ex.Message);
        }
    }

    public async Task<bool> IsMachineCollectedAsync(int machineId)
    {
        try
        {
            await client.Auth.Me.Collection.Machines[machineId].GetAsync();

            return true;
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 404)
        {
            return false;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error checking if machine {MachineId} is collected", machineId);

            return false;
        }
    }

    public async Task<(bool success, string? error)> AddMachineToCollectionAsync(int machineId)
    {
        try
        {
            await client.Auth.Me.Collection.Machines[machineId].PostAsync();

            return (true, null);
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 409)
        {
            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error adding machine {MachineId} to collection", machineId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error adding machine {MachineId} to collection", machineId);

            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> RemoveMachineFromCollectionAsync(int machineId)
    {
        try
        {
            await client.Auth.Me.Collection.Machines[machineId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error removing machine {MachineId} from collection", machineId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error removing machine {MachineId} from collection", machineId);

            return (false, ex.Message);
        }
    }

    public async Task<bool> IsSoftwareReleaseCollectedAsync(int releaseId)
    {
        try
        {
            await client.Auth.Me.Collection.SoftwareReleases[releaseId].GetAsync();

            return true;
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 404)
        {
            return false;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error checking if software release {ReleaseId} is collected", releaseId);

            return false;
        }
    }

    public async Task<(bool success, string? error)> AddSoftwareReleaseToCollectionAsync(int releaseId)
    {
        try
        {
            await client.Auth.Me.Collection.SoftwareReleases[releaseId].PostAsync();

            return (true, null);
        }
        catch(ApiException ex) when(ex.ResponseStatusCode == 409)
        {
            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error adding software release {ReleaseId} to collection", releaseId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error adding software release {ReleaseId} to collection", releaseId);

            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> RemoveSoftwareReleaseFromCollectionAsync(int releaseId)
    {
        try
        {
            await client.Auth.Me.Collection.SoftwareReleases[releaseId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogError(ex, "API error removing software release {ReleaseId} from collection", releaseId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error removing software release {ReleaseId} from collection", releaseId);

            return (false, ex.Message);
        }
    }
}
