using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database.Models;

namespace Marechai.Pages.Admin
{
    public partial class InstructionSets
    {
        bool                 _deleteInProgress;
        Modal                _frmDelete;
        InstructionSet       _instructionSet;
        List<InstructionSet> _instructionSets;

        void ShowModal(int itemId)
        {
            _instructionSet = _instructionSets.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_instructionSet is null)
                return;

            _deleteInProgress = true;
            _instructionSets  = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_instructionSet.Id);
            _instructionSets = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _instructionSet = null;

        protected override async Task OnInitializedAsync() => _instructionSets = await Service.GetAsync();
    }
}