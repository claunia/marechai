using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class DocumentPeople
    {
        bool                          _deleteInProgress;
        Modal                         _frmDelete;
        List<DocumentPersonViewModel> _people;
        DocumentPersonViewModel       _person;

        void ShowModal(int itemId)
        {
            _person = _people.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_person is null)
                return;

            _deleteInProgress = true;
            _people           = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_person.Id);
            _people = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _person = null;

        protected override async Task OnInitializedAsync() => _people = await Service.GetAsync();
    }
}