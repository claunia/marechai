using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class BooksListPage : Page
{
    public BooksListPage()
    {
        InitializeComponent();
        DataContextChanged += BooksListPage_DataContextChanged;
    }

    private void BooksListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is BooksListViewModel vm)
            vm.LoadData.Execute(null);
    }
}
