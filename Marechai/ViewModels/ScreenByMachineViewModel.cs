namespace Marechai.ViewModels
{
    public class ScreenByMachineViewModel : BaseViewModel<long>
    {
        public int             MachineId { get; set; }
        public ScreenViewModel Screen    { get; set; }
        public int             ScreenId  { get; set; }
    }
}