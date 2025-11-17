using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public class ShellViewModel
{
    private readonly INavigator _navigator;

    public ShellViewModel(INavigator navigator) => _navigator = navigator;
}