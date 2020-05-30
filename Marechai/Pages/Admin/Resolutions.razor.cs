using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class Resolutions
    {
        bool                      _deleteInProgress;
        Modal                     _frmDelete;
        ResolutionViewModel       _resolution;
        List<ResolutionViewModel> _resolutions;

        void ShowModal(int itemId)
        {
            _resolution = _resolutions.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_resolution is null)
                return;

            _deleteInProgress = true;
            _resolutions      = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_resolution.Id);
            _resolutions = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _resolution = null;

        protected override async Task OnInitializedAsync() => _resolutions = await Service.GetAsync();
    }
}