using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareScreenshotsPage : Page
{
    public AdminSoftwareScreenshotsPage()
    {
        InitializeComponent();
    }

    AdminSoftwareScreenshotsViewModel ViewModel => DataContext as AdminSoftwareScreenshotsViewModel;

    void PlatformAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            ViewModel?.UpdatePlatformSuggestions(sender.Text);
    }

    void PlatformAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender,
                                                  AutoSuggestBoxSuggestionChosenEventArgs args) =>
        ViewModel?.OnPlatformSuggestionChosen(args.SelectedItem as string);

    void VersionAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            ViewModel?.UpdateVersionSuggestions(sender.Text);
    }

    void VersionAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender,
                                                 AutoSuggestBoxSuggestionChosenEventArgs args) =>
        ViewModel?.OnVersionSuggestionChosen(args.SelectedItem as string);
}
