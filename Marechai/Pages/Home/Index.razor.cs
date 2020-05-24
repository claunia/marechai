using System.Collections.Generic;
using Marechai.ViewModels;

namespace Marechai.Pages.Home
{
    public partial class Index
    {
        List<NewsViewModel> _news;

        protected override void OnInitialized() => _news = Service.GetNews();
    }
}