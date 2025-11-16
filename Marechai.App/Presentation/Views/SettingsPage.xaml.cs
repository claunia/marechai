using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    public SettingsViewModel? ViewModel => DataContext as SettingsViewModel;
}