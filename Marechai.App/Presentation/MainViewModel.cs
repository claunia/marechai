using System.Threading.Tasks;
using System.Windows.Input;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string? name;
    [ObservableProperty]
    private NewsViewModel? newsViewModel;

    public MainViewModel(IStringLocalizer localizer, IOptions<AppConfig> appInfo, INavigator navigator,
                         NewsViewModel    newsViewModel)
    {
        _navigator    =  navigator;
        NewsViewModel =  newsViewModel;
        Title         =  "Marechai";
        Title         += $" - {localizer["ApplicationName"]}";
        Title         += $" - {appInfo?.Value?.Environment}";
        GoToSecond    =  new AsyncRelayCommand(GoToSecondView);
    }

    public string? Title { get; }

    public ICommand GoToSecond { get; }

    private async Task GoToSecondView()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
    }
}