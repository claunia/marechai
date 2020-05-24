using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class Machines
    {
        bool                   _deleteInProgress;
        Modal                  _frmDelete;
        MachineViewModel       _machine;
        List<MachineViewModel> _machines;

        void ShowModal(int itemId)
        {
            _machine = _machines.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_machine is null)
                return;

            _deleteInProgress = true;
            _machines         = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_machine.Id);
            _machines = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _machine = null;

        protected override async Task OnInitializedAsync() => _machines = await Service.GetAsync();
    }
}