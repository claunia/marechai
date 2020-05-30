namespace Marechai.ViewModels
{
    public class InstructionSetExtensionByProcessorViewModel : BaseViewModel<int>
    {
        public string Extension   { get; set; }
        public string Processor   { get; set; }
        public int    ProcessorId { get; set; }
        public int    ExtensionId { get; set; }
    }
}