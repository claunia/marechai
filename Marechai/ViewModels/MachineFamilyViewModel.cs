namespace Marechai.ViewModels
{
    public class MachineFamilyViewModel : BaseViewModel<int>
    {
        public string Company   { get; set; }
        public string Name      { get; set; }
        public int    CompanyId { get; set; }
    }
}