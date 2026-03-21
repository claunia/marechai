#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

public class PeopleService
{
    private readonly ApiClient              _apiClient;
    private readonly ILogger<PeopleService> _logger;

    public PeopleService(ApiClient apiClient, ILogger<PeopleService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
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
}
