using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class BooksPage : Page
{
    public BooksPage()
    {
        InitializeComponent();
        DataContextChanged += BooksPage_DataContextChanged;
        Loaded             += BooksPage_Loaded;
    }

    private void BooksPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not BooksViewModel viewModel) return;

        _ = viewModel.LoadData.ExecuteAsync(null);
    }

    private void BooksPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is BooksViewModel viewModel)
            _ = viewModel.LoadData.ExecuteAsync(null);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is BooksViewModel viewModel)
            _ = viewModel.LoadData.ExecuteAsync(null);
    }
}
