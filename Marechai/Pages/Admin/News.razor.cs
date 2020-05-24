using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class News
    {
        NewsViewModel       _currentNews;
        bool                _deleteInProgress;
        Modal               _frmDelete;
        List<NewsViewModel> _news;

        protected override async Task OnInitializedAsync() => _news = await Service.GetAsync();

        void ShowModal(int itemId)
        {
            _currentNews = _news.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_currentNews is null)
                return;

            _deleteInProgress = true;
            _news             = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_currentNews.Id);
            _news = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _currentNews = null;
    }
}