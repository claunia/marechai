namespace Marechai.ViewModels
{
    public class ResolutionByGpuViewModel : BaseViewModel<long>
    {
        public int                 ResolutionId { get; set; }
        public int                 GpuId        { get; set; }
        public ResolutionViewModel Resolution   { get; set; }
    }
}