using System.ComponentModel;

namespace Marechai.ViewModels
{
    public class DocumentCompanyViewModel : BaseViewModel<int>
    {
        public string Name { get; set; }
        [DisplayName("Linked company")]
        public string Company { get; set; }
        public int? CompanyId { get; set; }
    }
}