using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database.Models;

namespace Marechai.Pages.Admin
{
    public partial class Screens
    {
        bool         _deleteInProgress;
        Modal        _frmDelete;
        Screen       _screen;
        List<Screen> _screens;

        void ShowModal(int itemId)
        {
            _screen = _screens.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_screen is null)
                return;

            _deleteInProgress = true;
            _screens          = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_screen.Id);
            _screens = Service.Get();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _screen = null;

        protected override async Task OnInitializedAsync() => _screens = await Service.GetAsync();
    }
}