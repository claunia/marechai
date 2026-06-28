using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareCoversPage : Page
{
    public AdminSoftwareCoversPage()
    {
        InitializeComponent();
    }

    AdminSoftwareCoversViewModel ViewModel => DataContext as AdminSoftwareCoversViewModel;
}
