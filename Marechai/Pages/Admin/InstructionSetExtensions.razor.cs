using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database.Models;

namespace Marechai.Pages.Admin
{
    public partial class InstructionSetExtensions
    {
        bool                          _deleteInProgress;
        InstructionSetExtension       _extension;
        List<InstructionSetExtension> _extensions;
        Modal                         _frmDelete;

        void ShowModal(int itemId)
        {
            _extension = _extensions.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_extension is null)
                return;

            _deleteInProgress = true;
            _extensions       = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_extension.Id);
            _extensions = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _extension = null;

        protected override async Task OnInitializedAsync() => _extensions = await Service.GetAsync();
    }
}