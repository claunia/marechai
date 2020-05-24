using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database.Models;

namespace Marechai.Pages.Admin
{
    public partial class Licenses
    {
        bool          _deleteInProgress;
        Modal         _frmDelete;
        License       _license;
        List<License> _licenses;

        void ShowModal(int itemId)
        {
            _license = _licenses.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_license is null)
                return;

            _deleteInProgress = true;
            _licenses         = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_license.Id);
            _licenses = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _license = null;

        protected override async Task OnInitializedAsync() => _licenses = await Service.GetAsync();
    }
}