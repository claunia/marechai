using System.Threading.Tasks;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Machines
{
    public partial class View
    {
        bool[] _gpuVisible;
        bool   _loaded;

        MachineViewModel _machine;
        bool[]           _processorVisible;
        bool[]           _soundVisible;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _machine = await Service.GetMachine(Id);

            _processorVisible = new bool[_machine.Processors.Count];
            _gpuVisible       = new bool[_machine.Gpus.Count];
            _soundVisible     = new bool[_machine.SoundSynthesizers.Count];

            _loaded = true;
        }
    }
}