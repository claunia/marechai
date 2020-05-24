using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Consoles
{
    public partial class Search
    {
        char? _character;

        List<MachineViewModel> _consoles;
        [Parameter]
        public int? Year { get; set; }

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
                _consoles = await Service.GetConsolesByLetterAsync(_character.Value);

            if(Year.HasValue &&
               _consoles is null)
                _consoles = await Service.GetConsolesByYearAsync(Year.Value);

            _consoles ??= await Service.GetConsolesAsync();
        }
    }
}