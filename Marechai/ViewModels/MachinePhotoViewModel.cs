namespace Marechai.ViewModels
{
    public class MachinePhotoViewModel : BasePhotoViewModel
    {
        public string Source             { get; set; }
        public string MachineName        { get; set; }
        public string MachineCompanyName { get; set; }
        public int    MachineId          { get; set; }
    }
}