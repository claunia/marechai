namespace Marechai.ViewModels
{
    public class ResolutionByScreenViewModel : BaseViewModel<long>
    {
        public int                 ResolutionId { get; set; }
        public int                 ScreenId     { get; set; }
        public ResolutionViewModel Resolution   { get; set; }
    }
}