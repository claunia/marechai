using Marechai.App.Presentation.Views;

namespace Marechai.App.Presentation.ViewModels;

public partial class ContactViewModel : ObservableObject
{
    private readonly IRegionManager _regionManager;

    public ContactViewModel(IStringLocalizer localizer, IRegionManager regionManager)
    {
        Localizer      = localizer;
        _regionManager = regionManager;
    }

    public IStringLocalizer Localizer { get; }

    [RelayCommand]
    private void NavigateToAbout() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AboutPage));
}
