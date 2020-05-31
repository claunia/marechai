namespace Marechai.ViewModels
{
    public class CompanyDescriptionViewModel : BaseViewModel<int>
    {
        public string Markdown  { get; set; }
        public string Html      { get; set; }
        public int    CompanyId { get; set; }
    }
}