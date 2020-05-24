using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.Database.Models;

namespace Marechai.Pages.Admin
{
    public partial class BrowserTests
    {
        List<BrowserTest> _tests;

        protected override async Task OnInitializedAsync() => _tests = await Service.GetAsync();
    }
}