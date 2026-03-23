using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class PeoplePage : Page
{
    public PeoplePage()
    {
        InitializeComponent();
        DataContextChanged += PeoplePage_DataContextChanged;
    }

    private void PeoplePage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is PeopleViewModel viewModel)
            _ = viewModel.LoadData.ExecuteAsync(null);
    }
}
