#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class DocumentsService
{
    private readonly Client                   _apiClient;
    private readonly ILogger<DocumentsService>   _logger;

    public DocumentsService(Client apiClient, ILogger<DocumentsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- User-facing browsing methods ---

    public async Task<int> GetDocumentsCountAsync()
    {
        try
        {
            int? result = await _apiClient.Documents.Count.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents count");

            return 0;
        }
    }

    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            int? result = await _apiClient.Documents.MinimumYear.GetAsync();

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
            int? result = await _apiClient.Documents.MaximumYear.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching maximum year");

            return 0;
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsByLetterAsync(char letter)
    {
        try
        {
            List<DocumentDto>? docs = await _apiClient.Documents.ByLetter[letter.ToString()].GetAsync();

            return docs ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents by letter '{Letter}'", letter);

            return [];
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsByYearAsync(int year)
    {
        try
        {
            List<DocumentDto>? docs = await _apiClient.Documents.ByYear[year].GetAsync();

            return docs ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents by year {Year}", year);

            return [];
        }
    }

    public async Task<DocumentSynopsisDto?> GetDocumentSynopsisAsync(long documentId)
    {
        try
        {
            return await _apiClient.Documents[documentId].Synopsis.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching synopsis for document {Id}", documentId);

            return null;
        }
    }

    // --- CRUD ---

    public async Task<List<DocumentDto>> GetAllDocumentsAsync()
    {
        try
        {
            List<DocumentDto>? documents = await _apiClient.Documents.GetAsync();

            return documents ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents");

            return [];
        }
    }

    public async Task<DocumentDto?> GetDocumentAsync(long id)
    {
        try
        {
            return await _apiClient.Documents[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching document {Id}", id);

            return null;
        }
    }

    public async Task<long?> CreateDocumentAsync(DocumentDto dto)
    {
        try
        {
            return await _apiClient.Documents.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating document");

            return null;
        }
    }

    public async Task<bool> UpdateDocumentAsync(long id, DocumentDto dto)
    {
        try
        {
            await _apiClient.Documents[id].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating document {Id}", id);

            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(long id)
    {
        try
        {
            await _apiClient.Documents[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id}", id);

            return false;
        }
    }

    // --- Synopses ---
    public async Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long documentId)
    {
        try
        {
            List<DocumentSynopsisDto>? synopses = await _apiClient.Documents[documentId].Synopses.GetAsync();

            return synopses ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching synopses for document {Id}", documentId);

            return [];
        }
    }

    public async Task<long?> UpsertSynopsisAsync(long documentId, DocumentSynopsisDto dto)
    {
        try
        {
            return await _apiClient.Documents[documentId].Synopsis.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving synopsis for document {Id}", documentId);

            return null;
        }
    }

    public async Task<bool> DeleteSynopsisAsync(long documentId, string languageCode)
    {
        try
        {
            await _apiClient.Documents[documentId].Synopsis[languageCode].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting synopsis for document {Id} language {Lang}", documentId,
                             languageCode);

            return false;
        }
    }

    // --- People junction ---
    public async Task<List<PersonByDocumentDto>> GetPeopleByDocumentAsync(long documentId)
    {
        try
        {
            List<PersonByDocumentDto>? people = await _apiClient.Documents[documentId].People.GetAsync();

            return people ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people for document {Id}", documentId);

            return [];
        }
    }

    public async Task<long?> AddPersonToDocumentAsync(PersonByDocumentDto dto)
    {
        try
        {
            return await _apiClient.PeopleByDocument.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to document");

            return null;
        }
    }

    public async Task<bool> RemovePersonFromDocumentAsync(long id)
    {
        try
        {
            await _apiClient.PeopleByDocument[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing person from document {Id}", id);

            return false;
        }
    }

    // --- Companies junction ---
    public async Task<List<CompanyByDocumentDto>> GetCompaniesByDocumentAsync(long documentId)
    {
        try
        {
            List<CompanyByDocumentDto>? companies =
                await _apiClient.Documents[documentId].Companies.GetAsync();

            return companies ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for document {Id}", documentId);

            return [];
        }
    }

    public async Task<long?> AddCompanyToDocumentAsync(CompanyByDocumentDto dto)
    {
        try
        {
            return await _apiClient.Documents.Companies.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to document");

            return null;
        }
    }

    public async Task<bool> RemoveCompanyFromDocumentAsync(long id)
    {
        try
        {
            await _apiClient.Documents.Companies[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing company from document {Id}", id);

            return false;
        }
    }

    // --- Machines junction ---
    public async Task<List<DocumentByMachineDto>> GetMachinesByDocumentAsync(long documentId)
    {
        try
        {
            List<DocumentByMachineDto>? machines =
                await _apiClient.Documents[documentId].Machines.GetAsync();

            return machines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machines for document {Id}", documentId);

            return [];
        }
    }

    public async Task<long?> AddMachineToDocumentAsync(DocumentByMachineDto dto)
    {
        try
        {
            return await _apiClient.Machines.Documents.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine to document");

            return null;
        }
    }

    public async Task<bool> RemoveMachineFromDocumentAsync(long id)
    {
        try
        {
            await _apiClient.Machines.Documents[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing machine from document {Id}", id);

            return false;
        }
    }

    // --- Machine Families junction ---
    public async Task<List<DocumentByMachineFamilyDto>> GetMachineFamiliesByDocumentAsync(long documentId)
    {
        try
        {
            List<DocumentByMachineFamilyDto>? families =
                await _apiClient.Documents[documentId].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machine families for document {Id}", documentId);

            return [];
        }
    }

    public async Task<long?> AddMachineFamilyToDocumentAsync(DocumentByMachineFamilyDto dto)
    {
        try
        {
            return await _apiClient.MachineFamilies.Documents.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine family to document");

            return null;
        }
    }

    public async Task<bool> RemoveMachineFamilyFromDocumentAsync(long id)
    {
        try
        {
            await _apiClient.MachineFamilies.Documents[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing machine family from document {Id}", id);

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
