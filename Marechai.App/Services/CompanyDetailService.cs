#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching company details from the API
/// </summary>
public class CompanyDetailService
{
    private readonly Client                     _apiClient;
    private readonly ILogger<CompanyDetailService> _logger;

    public CompanyDetailService(Client apiClient, ILogger<CompanyDetailService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Gets a single company by ID with full details
    /// </summary>
    public async Task<CompanyDto?> GetCompanyByIdAsync(int companyId)
    {
        try
        {
            _logger.LogInformation("Fetching company {CompanyId} from API", companyId);

            CompanyDto? company = await _apiClient.Companies[companyId].GetAsync();

            if(company == null)
            {
                _logger.LogWarning("Company {CompanyId} not found", companyId);

                return null;
            }

            _logger.LogInformation("Successfully fetched company {CompanyId}: {CompanyName}", companyId, company.Name);

            return company;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching company {CompanyId} from API", companyId);

            return null;
        }
    }

    /// <summary>
    ///     Gets machines (computers) made by a company
    /// </summary>
    public async Task<List<MachineDto>> GetComputersByCompanyAsync(int companyId)
    {
        try
        {
            _logger.LogInformation("Fetching computers for company {CompanyId}", companyId);

            List<MachineDto>? machines = await _apiClient.Companies[companyId].Machines.GetAsync();

            if(machines == null) return [];

            _logger.LogInformation("Successfully fetched {Count} computers for company {CompanyId}",
                                   machines.Count,
                                   companyId);

            return machines;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching computers for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets the sold-to company (when company was sold, merged, or renamed)
    /// </summary>
    public async Task<CompanyDto?> GetSoldToCompanyAsync(int? companyId)
    {
        if(companyId is null or <= 0) return null;

        try
        {
            _logger.LogInformation("Fetching sold-to company {CompanyId}", companyId);

            CompanyDto? company = await _apiClient.Companies[companyId.Value].GetAsync();

            return company;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching sold-to company {CompanyId}", companyId);

            return null;
        }
    }

    /// <summary>
    ///     Gets all logos for a company
    /// </summary>
    public async Task<List<CompanyLogoDto>> GetCompanyLogosAsync(int companyId)
    {
        try
        {
            _logger.LogInformation("Fetching logos for company {CompanyId}", companyId);

            List<CompanyLogoDto>? logos = await _apiClient.Companies[companyId].Logos.GetAsync();

            if(logos == null) return [];

            _logger.LogInformation("Successfully fetched {Count} logos for company {CompanyId}",
                                   logos.Count,
                                   companyId);

            return logos;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching logos for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets the localized description for a company. Falls back to English on the server if the
    ///     requested language is not available.
    /// </summary>
    public async Task<CompanyDescriptionDto?> GetDescriptionAsync(int companyId, string languageCode)
    {
        try
        {
            _logger.LogInformation("Fetching description for company {CompanyId} in language {Lang}",
                                   companyId,
                                   languageCode);

            CompanyDescriptionDto? desc = await _apiClient.Companies[companyId].Description.GetAsync(
                                              config => config.QueryParameters.Lang = languageCode);

            return desc;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching description for company {CompanyId}", companyId);

            return null;
        }
    }

    /// <summary>
    ///     Gets people associated with a company
    /// </summary>
    public async Task<List<PersonByCompanyDto>> GetPeopleByCompanyAsync(int companyId)
    {
        try
        {
            _logger.LogInformation("Fetching people for company {CompanyId}", companyId);

            List<PersonByCompanyDto>? people = await _apiClient.Companies[companyId].People.GetAsync();

            if(people == null) return [];

            _logger.LogInformation("Successfully fetched {Count} people for company {CompanyId}",
                                   people.Count,
                                   companyId);

            return people;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Adds a person to a company
    /// </summary>
    public async Task<long?> AddPersonToCompanyAsync(PersonByCompanyDto dto)
    {
        try
        {
            return await _apiClient.PeopleByCompany.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to company");

            return null;
        }
    }

    /// <summary>
    ///     Updates a person-company association
    /// </summary>
    public async Task<bool> UpdatePersonInCompanyAsync(long id, PersonByCompanyDto dto)
    {
        try
        {
            await _apiClient.PeopleByCompany[id].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating person-company association {Id}", id);

            return false;
        }
    }

    /// <summary>
    ///     Removes a person from a company
    /// </summary>
    public async Task<bool> RemovePersonFromCompanyAsync(long id)
    {
        try
        {
            await _apiClient.PeopleByCompany[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing person from company {Id}", id);

            return false;
        }
    }

    /// <summary>
    ///     Gets machine families designed by a company
    /// </summary>
    public async Task<List<MachineFamilyDto>> GetMachineFamiliesAsync(int companyId)
    {
        try
        {
            List<MachineFamilyDto>? families = await _apiClient.Companies[companyId].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machine families for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets GPUs manufactured by a company
    /// </summary>
    public async Task<List<GpuDto>> GetGpusAsync(int companyId)
    {
        try
        {
            List<GpuDto>? gpus = await _apiClient.Companies[companyId].Gpus.GetAsync();

            return gpus ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching GPUs for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets processors manufactured by a company
    /// </summary>
    public async Task<List<ProcessorDto>> GetProcessorsAsync(int companyId)
    {
        try
        {
            List<ProcessorDto>? processors = await _apiClient.Companies[companyId].Processors.GetAsync();

            return processors ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching processors for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets sound synthesizers made by a company
    /// </summary>
    public async Task<List<SoundSynthDto>> GetSoundSynthsAsync(int companyId)
    {
        try
        {
            List<SoundSynthDto>? soundSynths = await _apiClient.Companies[companyId].SoundSynths.GetAsync();

            return soundSynths ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching sound synthesizers for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets software related to a company
    /// </summary>
    public async Task<List<SoftwareDto>> GetSoftwareAsync(int companyId)
    {
        try
        {
            List<SoftwareDto>? software = await _apiClient.Companies[companyId].Software.GetAsync();

            return software ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets books related to a company
    /// </summary>
    public async Task<List<BookDto>> GetBooksAsync(int companyId)
    {
        try
        {
            List<BookDto>? books = await _apiClient.Companies[companyId].Books.GetAsync();

            return books ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching books for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets documents related to a company
    /// </summary>
    public async Task<List<DocumentDto>> GetDocumentsAsync(int companyId)
    {
        try
        {
            List<DocumentDto>? documents = await _apiClient.Companies[companyId].Documents.GetAsync();

            return documents ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents for company {CompanyId}", companyId);

            return [];
        }
    }

    /// <summary>
    ///     Gets magazines related to a company
    /// </summary>
    public async Task<List<MagazineDto>> GetMagazinesAsync(int companyId)
    {
        try
        {
            List<MagazineDto>? magazines = await _apiClient.Companies[companyId].Magazines.GetAsync();

            return magazines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching magazines for company {CompanyId}", companyId);

            return [];
        }
    }
}