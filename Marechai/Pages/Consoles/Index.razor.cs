using System.Threading.Tasks;

namespace Marechai.Pages.Consoles
{
    public partial class Index
    {
        int  _consoles;
        bool _loaded;
        int  _maxYear;
        int  _minYear;

        protected override async Task OnInitializedAsync()
        {
            _consoles = await Service.GetConsolesCountAsync();
            _minYear  = await Service.GetMinimumYearAsync();
            _maxYear  = await Service.GetMaximumYearAsync();

            _loaded = true;
        }
    }
}