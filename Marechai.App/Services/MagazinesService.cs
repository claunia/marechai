#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class MagazinesService
{
    private readonly Client                    _apiClient;
    private readonly ILogger<MagazinesService>    _logger;

    public MagazinesService(Client apiClient, ILogger<MagazinesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- User-facing browsing methods ---

    public async Task<int> GetMagazinesCountAsync()
    {
        try
        {
            int? result = await _apiClient.Magazines.Count.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching magazines count");

            return 0;
        }
    }

    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            int? result = await _apiClient.Magazines.MinimumYear.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching minimum year");

            return 0;
        }
    }

    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            int? result = await _apiClient.Magazines.MaximumYear.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching maximum year");

            return 0;
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesByLetterAsync(char letter)
    {
        try
        {
            List<MagazineDto>? mags = await _apiClient.Magazines.ByLetter[letter.ToString()].GetAsync();

            return mags ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching magazines by letter '{Letter}'", letter);

            return [];
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesByYearAsync(int year)
    {
        try
        {
            List<MagazineDto>? mags = await _apiClient.Magazines.ByYear[year].GetAsync();

            return mags ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching magazines by year {Year}", year);

            return [];
        }
    }

    public async Task<MagazineIssueFullDto?> GetIssueFullAsync(long issueId)
    {
        try
        {
            return await _apiClient.Magazines.Issues[issueId].Full.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching full issue {Id}", issueId);

            return null;
        }
    }

    public async Task<bool> IsMagazineIssueCollectedAsync(long issueId)
    {
        try
        {
            bool? result = await _apiClient.Auth.Me.Collection.MagazineIssues[issueId].GetAsync();

            return result ?? false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error checking collection state for issue {Id}", issueId);

            return false;
        }
    }

    public async Task<bool> AddMagazineIssueToCollectionAsync(long issueId)
    {
        try
        {
            await _apiClient.Auth.Me.Collection.MagazineIssues[issueId].PostAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding issue {Id} to collection", issueId);

            return false;
        }
    }

    public async Task<bool> RemoveMagazineIssueFromCollectionAsync(long issueId)
    {
        try
        {
            await _apiClient.Auth.Me.Collection.MagazineIssues[issueId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing issue {Id} from collection", issueId);

            return false;
        }
    }

    public async Task<DocumentSynopsisDto?> GetMagazineSynopsisAsync(long magazineId)
    {
        try
        {
            return await _apiClient.Magazines[magazineId].Synopsis.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching synopsis for magazine {Id}", magazineId);

            return null;
        }
    }

    // --- CRUD ---

    public async Task<List<MagazineDto>> GetAllMagazinesAsync()
    {
        try
        {
            List<MagazineDto>? magazines = await _apiClient.Magazines.GetAsync();

            return magazines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching magazines");

            return [];
        }
    }

    public async Task<MagazineDto?> GetMagazineAsync(long id)
    {
        try
        {
            return await _apiClient.Magazines[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching magazine {Id}", id);

            return null;
        }
    }

    public async Task<long?> CreateMagazineAsync(MagazineDto dto)
    {
        try
        {
            return await _apiClient.Magazines.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating magazine");

            return null;
        }
    }

    public async Task<bool> UpdateMagazineAsync(long id, MagazineDto dto)
    {
        try
        {
            await _apiClient.Magazines[id].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating magazine {Id}", id);

            return false;
        }
    }

    public async Task<bool> DeleteMagazineAsync(long id)
    {
        try
        {
            await _apiClient.Magazines[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting magazine {Id}", id);

            return false;
        }
    }

    // --- Synopses ---
    public async Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long magazineId)
    {
        try
        {
            List<DocumentSynopsisDto>? synopses = await _apiClient.Magazines[magazineId].Synopses.GetAsync();

            return synopses ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching synopses for magazine {Id}", magazineId);

            return [];
        }
    }

    public async Task<long?> UpsertSynopsisAsync(long magazineId, DocumentSynopsisDto dto)
    {
        try
        {
            return await _apiClient.Magazines[magazineId].Synopsis.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving synopsis for magazine {Id}", magazineId);

            return null;
        }
    }

    public async Task<bool> DeleteSynopsisAsync(long magazineId, string languageCode)
    {
        try
        {
            await _apiClient.Magazines[magazineId].Synopsis[languageCode].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting synopsis for magazine {Id} language {Lang}", magazineId,
                             languageCode);

            return false;
        }
    }

    // --- People junction ---
    public async Task<List<PersonByMagazineDto>> GetPeopleByMagazineAsync(long magazineId)
    {
        try
        {
            List<PersonByMagazineDto>? people = await _apiClient.Magazines[magazineId].People.GetAsync();

            return people ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people for magazine {Id}", magazineId);

            return [];
        }
    }

    public async Task<long?> AddPersonToMagazineAsync(PersonByMagazineDto dto)
    {
        try
        {
            return await _apiClient.PeopleByMagazine.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to magazine");

            return null;
        }
    }

    public async Task<bool> RemovePersonFromMagazineAsync(long id)
    {
        try
        {
            await _apiClient.PeopleByMagazine[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing person from magazine {Id}", id);

            return false;
        }
    }

    // --- Companies junction ---
    public async Task<List<CompanyByMagazineDto>> GetCompaniesByMagazineAsync(long magazineId)
    {
        try
        {
            List<CompanyByMagazineDto>? companies =
                await _apiClient.Magazines[magazineId].Companies.GetAsync();

            return companies ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for magazine {Id}", magazineId);

            return [];
        }
    }

    public async Task<long?> AddCompanyToMagazineAsync(CompanyByMagazineDto dto)
    {
        try
        {
            return await _apiClient.Magazines.Companies.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to magazine");

            return null;
        }
    }

    public async Task<bool> RemoveCompanyFromMagazineAsync(long id)
    {
        try
        {
            await _apiClient.Magazines.Companies[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing company from magazine {Id}", id);

            return false;
        }
    }

    // --- Machines junction ---
    public async Task<List<MagazineByMachineDto>> GetMachinesByMagazineAsync(long magazineId)
    {
        try
        {
            List<MagazineByMachineDto>? machines =
                await _apiClient.Magazines[magazineId].Machines.GetAsync();

            return machines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machines for magazine {Id}", magazineId);

            return [];
        }
    }

    public async Task<long?> AddMachineToMagazineAsync(MagazineByMachineDto dto)
    {
        try
        {
            return await _apiClient.MagazinesByMachine.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine to magazine");

            return null;
        }
    }

    public async Task<bool> RemoveMachineFromMagazineAsync(long id)
    {
        try
        {
            await _apiClient.MagazinesByMachine[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing machine from magazine {Id}", id);

            return false;
        }
    }

    // --- Machine Families junction ---
    public async Task<List<MagazineByMachineFamilyDto>> GetMachineFamiliesByMagazineAsync(long magazineId)
    {
        try
        {
            List<MagazineByMachineFamilyDto>? families =
                await _apiClient.Magazines[magazineId].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machine families for magazine {Id}", magazineId);

            return [];
        }
    }

    public async Task<long?> AddMachineFamilyToMagazineAsync(MagazineByMachineFamilyDto dto)
    {
        try
        {
            return await _apiClient.MagazinesByMachineFamily.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine family to magazine");

            return null;
        }
    }

    public async Task<bool> RemoveMachineFamilyFromMagazineAsync(long id)
    {
        try
        {
            await _apiClient.MagazinesByMachineFamily[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing machine family from magazine {Id}", id);

            return false;
        }
    }

    // --- Document Roles ---
    public async Task<List<DocumentRoleDto>> GetDocumentRolesAsync()
    {
        try
        {
            List<DocumentRoleDto>? roles = await _apiClient.Documents.Roles.Enabled.GetAsync();

            return roles ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching document roles");

            return [];
        }
    }
}
