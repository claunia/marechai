namespace Marechai.App.Presentation.ViewModels;

public class ShellViewModel
{
    private readonly IRegionManager _regionManager;

    public ShellViewModel(IRegionManager regionManager) => _regionManager = regionManager;
}