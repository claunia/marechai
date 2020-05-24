using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class SoundSynths
    {
        bool                      _deleteInProgress;
        Modal                     _frmDelete;
        SoundSynthViewModel       _soundSynth;
        List<SoundSynthViewModel> _soundSynths;

        void ShowModal(int itemId)
        {
            _soundSynth = _soundSynths.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_soundSynth is null)
                return;

            _deleteInProgress = true;
            _soundSynths      = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_soundSynth.Id);
            _soundSynths = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _soundSynth = null;

        protected override async Task OnInitializedAsync() => _soundSynths = await Service.GetAsync();
    }
}