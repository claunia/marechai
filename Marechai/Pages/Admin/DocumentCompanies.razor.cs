using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;

namespace Marechai.Pages.Admin
{
    public partial class DocumentCompanies
    {
        List<DocumentCompanyViewModel> _companies;

        DocumentCompanyViewModel _currentCompany;
        bool                     _deleteInProgress;
        Modal                    _frmDelete;

        protected override async Task OnInitializedAsync() => _companies = await Service.GetAsync();

        void ShowModal(int itemId)
        {
            _currentCompany = _companies.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_currentCompany is null)
                return;

            _deleteInProgress = true;
            _companies        = null;

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_currentCompany.Id);
            _companies = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _currentCompany = null;
    }
}