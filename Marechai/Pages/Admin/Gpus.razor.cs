using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class Gpus
    {
        bool               _deleteInProgress;
        Modal              _frmDelete;
        GpuViewModel       _gpu;
        List<GpuViewModel> _gpus;

        void ShowModal(int itemId)
        {
            _gpu = _gpus.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_gpu is null)
                return;

            _deleteInProgress = true;
            _gpus             = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_gpu.Id);
            _gpus = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _gpu = null;

        protected override async Task OnInitializedAsync() => _gpus = await Service.GetAsync();
    }
}