using System.Threading.Tasks;

namespace Marechai.Pages.Computers
{
    public partial class Index
    {
        int  _computers;
        bool _loaded;
        int  _maxYear;
        int  _minYear;

        protected override async Task OnInitializedAsync()
        {
            _computers = await Service.GetComputersCountAsync();
            _minYear   = await Service.GetMinimumYearAsync();
            _maxYear   = await Service.GetMaximumYearAsync();

            _loaded = true;
        }
    }
}