using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class MagazinesListPage : Page
{
    public MagazinesListPage()
    {
        InitializeComponent();
        DataContextChanged += MagazinesListPage_DataContextChanged;
    }

    private void MagazinesListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is MagazinesListViewModel vm)
            vm.LoadData.Execute(null);
    }
}
