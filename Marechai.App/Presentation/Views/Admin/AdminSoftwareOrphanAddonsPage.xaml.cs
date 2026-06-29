using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareOrphanAddonsPage : Page
{
    public AdminSoftwareOrphanAddonsPage() => InitializeComponent();

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if(e.Key != Windows.System.VirtualKey.Enter) return;
        if(DataContext is not AdminSoftwareOrphanAddonsViewModel vm) return;
        if(sender is not TextBox textBox) return;

        vm.SearchText = textBox.Text;
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareOrphanAddonsViewModel vm) return;
        if(sender is not TextBox textBox) return;

        vm.SearchText = textBox.Text;
    }

    private void RowCheckBox_Toggled(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareOrphanAddonsViewModel vm) return;

        vm.OnRowSelectionChanged();
    }

    private void LinkRow_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareOrphanAddonsViewModel vm) return;
        if(sender is not FrameworkElement { DataContext: OrphanAddonRow row }) return;

        vm.OpenLinkDialogCommand.Execute(row);
    }

    private void OpenInNewWindow_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareOrphanAddonsViewModel vm) return;
        if(sender is not FrameworkElement { DataContext: OrphanAddonRow row }) return;

        vm.OpenInNewWindowCommand.Execute(row);
    }

    private void LinkTargetFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareOrphanAddonsViewModel vm)
            vm.UpdateLinkCandidates(sender.Text);
    }
}
