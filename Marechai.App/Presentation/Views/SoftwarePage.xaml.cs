#nullable enable

using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class SoftwarePage : Page
{
    public SoftwarePage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is SoftwareViewModel vm)
            await vm.LoadData.ExecuteAsync(null);
    }
}
