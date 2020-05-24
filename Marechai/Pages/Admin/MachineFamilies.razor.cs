using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class MachineFamilies
    {
        bool                         _deleteInProgress;
        Modal                        _frmDelete;
        List<MachineFamilyViewModel> _machineFamilies;
        MachineFamilyViewModel       _machineFamily;

        void ShowModal(int itemId)
        {
            _machineFamily = _machineFamilies.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_machineFamily is null)
                return;

            _deleteInProgress = true;
            _machineFamilies  = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_machineFamily.Id);
            _machineFamilies = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _machineFamily = null;

        protected override async Task OnInitializedAsync() => _machineFamilies = await Service.GetAsync();
    }
}