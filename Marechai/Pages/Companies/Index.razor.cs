using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Companies
{
    public partial class Index
    {
        char? _character;

        List<CompanyViewModel> _companies;
        string                 _countryName;
        [Parameter]
        public int? CountryId { get; set; }

        [Parameter]
        public string StartingCharacter { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _character = null;

            if(!string.IsNullOrWhiteSpace(StartingCharacter) &&
               StartingCharacter.Length == 1)
            {
                _character = StartingCharacter[0];

                // ToUpper()
                if(_character >= 'a' &&
                   _character <= 'z')
                    _character -= (char)32;

                // Check if not letter or number
                if(_character < '0'                       ||
                   (_character > '9' && _character < 'A') ||
                   _character > 'Z')
                    _character = null;
            }

            if(_character.HasValue)
                _companies = await Service.GetCompaniesByLetterAsync(_character.Value);

            if(CountryId.HasValue &&
               _companies is null)
            {
                _countryName = await Service.GetCountryNameAsync(CountryId.Value);

                if(_countryName != null)
                {
                    _companies = await Service.GetCompaniesByCountryAsync(CountryId.Value);
                }
                else
                    CountryId = null;
            }

            _companies ??= await Service.GetAsync();
        }
    }
}