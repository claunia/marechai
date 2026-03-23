using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class MagazinesPage : Page
{
    public MagazinesPage()
    {
        InitializeComponent();
        DataContextChanged += MagazinesPage_DataContextChanged;
    }

    private void MagazinesPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is MagazinesViewModel viewModel)
            _ = viewModel.LoadData.ExecuteAsync(null);
    }
}
