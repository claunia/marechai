using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class DocumentsListPage : Page
{
    public DocumentsListPage()
    {
        InitializeComponent();
        DataContextChanged += DocumentsListPage_DataContextChanged;
    }

    private void DocumentsListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is DocumentsListViewModel vm)
            vm.LoadData.Execute(null);
    }
}
