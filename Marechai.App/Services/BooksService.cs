#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class BooksService
{
    private readonly Client              _apiClient;
    private readonly ILogger<BooksService>  _logger;

    public BooksService(Client apiClient, ILogger<BooksService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- User-facing browsing methods ---

    public async Task<int> GetBooksCountAsync()
    {
        try
        {
            int? result = await _apiClient.Books.Count.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching books count");

            return 0;
        }
    }

    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            int? result = await _apiClient.Books.MinimumYear.GetAsync();

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
            int? result = await _apiClient.Books.MaximumYear.GetAsync();

            return result ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching maximum year");

            return 0;
        }
    }

    public async Task<List<BookDto>> GetBooksByLetterAsync(char letter)
    {
        try
        {
            List<BookDto>? books = await _apiClient.Books.ByLetter[letter.ToString()].GetAsync();

            return books ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching books by letter '{Letter}'", letter);

            return [];
        }
    }

    public async Task<List<BookDto>> GetBooksByYearAsync(int year)
    {
        try
        {
            List<BookDto>? books = await _apiClient.Books.ByYear[year].GetAsync();

            return books ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching books by year {Year}", year);

            return [];
        }
    }

    public async Task<DocumentSynopsisDto?> GetBookSynopsisAsync(long bookId)
    {
        try
        {
            return await _apiClient.Books[bookId].Synopsis.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching synopsis for book {Id}", bookId);

            return null;
        }
    }

    // --- CRUD ---

    public async Task<List<BookDto>> GetAllBooksAsync()
    {
        try
        {
            List<BookDto>? books = await _apiClient.Books.GetAsync();

            return books ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching books");

            return [];
        }
    }

    public async Task<BookDto?> GetBookAsync(long id)
    {
        try
        {
            return await _apiClient.Books[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching book {Id}", id);

            return null;
        }
    }

    public async Task<long?> CreateBookAsync(BookDto dto)
    {
        try
        {
            return await _apiClient.Books.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating book");

            return null;
        }
    }

    public async Task<bool> UpdateBookAsync(long id, BookDto dto)
    {
        try
        {
            await _apiClient.Books[id].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating book {Id}", id);

            return false;
        }
    }

    public async Task<bool> DeleteBookAsync(long id)
    {
        try
        {
            await _apiClient.Books[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting book {Id}", id);

            return false;
        }
    }

    // --- Synopses ---
    public async Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long bookId)
    {
        try
        {
            List<DocumentSynopsisDto>? synopses = await _apiClient.Books[bookId].Synopses.GetAsync();

            return synopses ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching synopses for book {Id}", bookId);

            return [];
        }
    }

    public async Task<long?> UpsertSynopsisAsync(long bookId, DocumentSynopsisDto dto)
    {
        try
        {
            return await _apiClient.Books[bookId].Synopsis.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving synopsis for book {Id}", bookId);

            return null;
        }
    }

    public async Task<bool> DeleteSynopsisAsync(long bookId, string languageCode)
    {
        try
        {
            await _apiClient.Books[bookId].Synopsis[languageCode].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting synopsis for book {Id} language {Lang}", bookId, languageCode);

            return false;
        }
    }

    // --- People junction ---
    public async Task<List<PersonByBookDto>> GetPeopleByBookAsync(long bookId)
    {
        try
        {
            List<PersonByBookDto>? people = await _apiClient.Books[bookId].People.GetAsync();

            return people ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching people for book {Id}", bookId);

            return [];
        }
    }

    public async Task<long?> AddPersonToBookAsync(PersonByBookDto dto)
    {
        try
        {
            return await _apiClient.PeopleByBook.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to book");

            return null;
        }
    }

    public async Task<bool> RemovePersonFromBookAsync(long id)
    {
        try
        {
            await _apiClient.PeopleByBook[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing person from book {Id}", id);

            return false;
        }
    }

    // --- Companies junction ---
    public async Task<List<CompanyByBookDto>> GetCompaniesByBookAsync(long bookId)
    {
        try
        {
            List<CompanyByBookDto>? companies = await _apiClient.Books[bookId].Companies.GetAsync();

            return companies ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for book {Id}", bookId);

            return [];
        }
    }

    public async Task<long?> AddCompanyToBookAsync(CompanyByBookDto dto)
    {
        try
        {
            return await _apiClient.Books.Companies.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to book");

            return null;
        }
    }

    public async Task<bool> RemoveCompanyFromBookAsync(long id)
    {
        try
        {
            await _apiClient.Books.Companies[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing company from book {Id}", id);

            return false;
        }
    }

    // --- Machines junction ---
    public async Task<List<BookByMachineDto>> GetMachinesByBookAsync(long bookId)
    {
        try
        {
            List<BookByMachineDto>? machines = await _apiClient.Books[bookId].Machines.GetAsync();

            return machines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machines for book {Id}", bookId);

            return [];
        }
    }

    public async Task<int?> AddMachineToBookAsync(BookByMachineDto dto)
    {
        try
        {
            return await _apiClient.Machines.Books.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine to book");

            return null;
        }
    }

    public async Task<bool> RemoveMachineFromBookAsync(long id)
    {
        try
        {
            await _apiClient.Machines.Books[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing machine from book {Id}", id);

            return false;
        }
    }

    // --- Machine Families junction ---
    public async Task<List<BookByMachineFamilyDto>> GetMachineFamiliesByBookAsync(long bookId)
    {
        try
        {
            List<BookByMachineFamilyDto>? families =
                await _apiClient.Books[bookId].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machine families for book {Id}", bookId);

            return [];
        }
    }

    public async Task<int?> AddMachineFamilyToBookAsync(BookByMachineFamilyDto dto)
    {
        try
        {
            return await _apiClient.MachineFamilies.Books.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine family to book");

            return null;
        }
    }

    public async Task<bool> RemoveMachineFamilyFromBookAsync(long id)
    {
        try
        {
            await _apiClient.MachineFamilies.Books[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing machine family from book {Id}", id);

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

    // --- Cover ---
    public async Task<BookDto?> UploadCoverAsync(long bookId, byte[] fileBytes, string fileName)
    {
        try
        {
            _logger.LogInformation("Uploading cover for book {BookId}", bookId);

            string contentType = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            var body = new MultipartBody();
            body.AddOrReplacePart("file", contentType, new global::System.IO.MemoryStream(fileBytes), fileName);

            BookDto? result = await _apiClient.Books[bookId].Cover.Upload.PostAsync(body);

            _logger.LogInformation("Successfully uploaded cover for book {BookId}", bookId);

            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading cover for book {BookId}", bookId);

            return null;
        }
    }

    public async Task<bool> DeleteCoverAsync(long bookId)
    {
        try
        {
            _logger.LogInformation("Deleting cover for book {BookId}", bookId);
            await _apiClient.Books[bookId].Cover.DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover for book {BookId}", bookId);

            return false;
        }
    }
}
