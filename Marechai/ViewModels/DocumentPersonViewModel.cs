using System.ComponentModel;

namespace Marechai.ViewModels
{
    public class DocumentPersonViewModel : BaseViewModel<int>
    {
        public string Name { get; set; }
        public string Person { get; set; }
        public int? PersonId { get; set; }
        public string Alias { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get; set; }
    }
}