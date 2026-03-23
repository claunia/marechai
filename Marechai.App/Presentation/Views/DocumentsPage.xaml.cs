using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class DocumentsPage : Page
{
    public DocumentsPage()
    {
        InitializeComponent();
        DataContextChanged += DocumentsPage_DataContextChanged;
    }

    private void DocumentsPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is DocumentsViewModel viewModel)
            _ = viewModel.LoadData.ExecuteAsync(null);
    }
}
