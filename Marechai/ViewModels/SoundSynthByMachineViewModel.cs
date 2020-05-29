namespace Marechai.ViewModels
{
    public class SoundSynthByMachineViewModel : BaseViewModel<long>
    {
        public int    SoundSynthId { get; set; }
        public int    MachineId    { get; set; }
        public string CompanyName  { get; set; }
        public string Name         { get; set; }
    }
}