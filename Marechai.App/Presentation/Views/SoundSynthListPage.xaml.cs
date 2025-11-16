using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class SoundSynthListPage : Page
{
    public SoundSynthListPage()
    {
        InitializeComponent();
        Loaded             += SoundSynthListPage_Loaded;
        DataContextChanged += SoundSynthListPage_DataContextChanged;
    }

    private void SoundSynthListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is SoundSynthsListViewModel vm)
        {
            // Load data when DataContext is set
            vm.LoadData.Execute(null);
        }
    }

    private void SoundSynthListPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is SoundSynthsListViewModel vm)
        {
            // Load data when page is loaded (fallback)
            vm.LoadData.Execute(null);
        }
    }
}