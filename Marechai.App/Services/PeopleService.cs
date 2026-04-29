#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

public class PeopleService
{
    private readonly Client              _apiClient;
    private readonly ILogger<PeopleService> _logger;

    public PeopleService(Client apiClient, ILogger<PeopleService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<int> GetPeopleCountAsync()
    {
        try
        {
            int? result = await _apiClient.People.Count.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people count");

            return 0;
        }
    }

    public async Task<int> GetMinimumBirthYearAsync()
    {
        try
        {
            int? result = await _apiClient.People.MinimumYear.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching minimum birth year");

            return 0;
        }
    }

    public async Task<int> GetMaximumBirthYearAsync()
    {
        try
        {
            int? result = await _apiClient.People.MaximumYear.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching maximum birth year");

            return 0;
        }
    }

    public async Task<List<PersonDto>> GetPeopleByLetterAsync(char letter)
    {
        try
        {
            List<PersonDto>? people = await _apiClient.People.ByLetter[letter.ToString()].GetAsync();

            return people ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people by letter '{Letter}'", letter);

            return [];
        }
    }

    public async Task<List<PersonDto>> GetPeopleByYearAsync(int year)
    {
        try
        {
            List<PersonDto>? people = await _apiClient.People.ByYear[year].GetAsync();

            return people ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people by year {Year}", year);

            return [];
        }
    }

    public async Task<List<PersonDto>> GetAllPeopleAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all people from API");

            List<PersonDto>? people = await _apiClient.People.GetAsync();

            if(people == null) return [];

            _logger.LogInformation("Successfully fetched {Count} people", people.Count);

            return people;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people from API");

            return [];
        }
    }

    public async Task<PersonDto?> GetPersonByIdAsync(int personId)
    {
        try
        {
            _logger.LogInformation("Fetching person {PersonId} from API", personId);

            PersonDto? person = await _apiClient.People[personId].GetAsync();

            if(person == null)
            {
                _logger.LogWarning("Person {PersonId} not found", personId);

                return null;
            }

            return person;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching person {PersonId} from API", personId);

            return null;
        }
    }

    public async Task<List<PersonByBookDto>> GetBooksByPersonAsync(int personId)
    {
        try
        {
            List<PersonByBookDto>? books = await _apiClient.People[personId].Books.GetAsync();

            return books ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching books for person {PersonId}", personId);

            return [];
        }
    }

    public async Task<List<PersonByDocumentDto>> GetDocumentsByPersonAsync(int personId)
    {
        try
        {
            List<PersonByDocumentDto>? documents = await _apiClient.People[personId].Documents.GetAsync();

            return documents ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents for person {PersonId}", personId);

            return [];
        }
    }

    public async Task<List<PersonByMagazineDto>> GetMagazinesByPersonAsync(int personId)
    {
        try
        {
            List<PersonByMagazineDto>? magazines = await _apiClient.People[personId].Magazines.GetAsync();

            return magazines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching magazines for person {PersonId}", personId);

            return [];
        }
    }

    public async Task<List<PersonByCompanyDto>> GetCompaniesByPersonAsync(int personId)
    {
        try
        {
            List<PersonByCompanyDto>? companies = await _apiClient.People[personId].Companies.GetAsync();

            return companies ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for person {PersonId}", personId);

            return [];
        }
    }

    public async Task<List<PersonBySoftwareDto>> GetSoftwareByPersonAsync(int personId)
    {
        try
        {
            List<PersonBySoftwareDto>? software = await _apiClient.People[personId].Software.GetAsync();

            return software ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software credits for person {PersonId}", personId);

            return [];
        }
    }
}
