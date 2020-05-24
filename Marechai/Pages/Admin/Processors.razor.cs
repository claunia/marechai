using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class Processors
    {
        bool                     _deleteInProgress;
        Modal                    _frmDelete;
        ProcessorViewModel       _processor;
        List<ProcessorViewModel> _processors;

        void ShowModal(int itemId)
        {
            _processor = _processors.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_processor is null)
                return;

            _deleteInProgress = true;
            _processors       = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_processor.Id);
            _processors = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _processor = null;

        protected override async Task OnInitializedAsync() => _processors = await Service.GetAsync();
    }
}