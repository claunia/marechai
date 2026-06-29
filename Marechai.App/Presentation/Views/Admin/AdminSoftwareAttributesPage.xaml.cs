#nullable enable

using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareAttributesPage : Page
{
    public AdminSoftwareAttributesPage() => InitializeComponent();

    AdminSoftwareAttributesViewModel? ViewModel => DataContext as AdminSoftwareAttributesViewModel;

    async void FilterSoftwareBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(ViewModel == null) return;

        ViewModel.UpdateFilterSoftwareSuggestions(sender.Text);
        ViewModel.SyncFilterSoftwareSelection();

        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && string.IsNullOrWhiteSpace(sender.Text))
            await ViewModel.SelectFilterSoftwareAsync(null);
    }

    async void FilterSoftwareBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(ViewModel != null && args.SelectedItem is SoftwareDto software)
            await ViewModel.SelectFilterSoftwareAsync(software);
    }

    async void FilterSoftwareBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(ViewModel == null) return;

        if(args.ChosenSuggestion is SoftwareDto software)
        {
            await ViewModel.SelectFilterSoftwareAsync(software);

            return;
        }

        await ViewModel.ResolveFilterSoftwareQueryAsync(args.QueryText);
    }

    async void FilterReleaseBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(ViewModel == null) return;

        ViewModel.UpdateFilterReleaseSuggestions(sender.Text);
        ViewModel.SyncFilterReleaseSelection();

        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && string.IsNullOrWhiteSpace(sender.Text))
            await ViewModel.SelectFilterReleaseAsync(null);
    }

    async void FilterReleaseBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(ViewModel != null && args.SelectedItem is SoftwareReleaseLookupDto release)
            await ViewModel.SelectFilterReleaseAsync(release);
    }

    async void FilterReleaseBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(ViewModel == null) return;

        if(args.ChosenSuggestion is SoftwareReleaseLookupDto release)
        {
            await ViewModel.SelectFilterReleaseAsync(release);

            return;
        }

        await ViewModel.ResolveFilterReleaseQueryAsync(args.QueryText);
    }

    async void FilterCategoryBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(ViewModel != null && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            await ViewModel.UpdateFilterCategorySuggestionsAsync(sender.Text);
    }

    async void FilterCategoryBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(ViewModel != null && args.SelectedItem is string category)
            await ViewModel.CommitFilterCategoryAsync(category);
    }

    async void FilterCategoryBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(ViewModel != null)
            await ViewModel.CommitFilterCategoryAsync(args.QueryText);
    }

    async void FilterKeyBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(ViewModel != null && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            await ViewModel.UpdateFilterKeySuggestionsAsync(sender.Text);
    }

    async void FilterKeyBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(ViewModel != null && args.SelectedItem is string key)
            await ViewModel.CommitFilterKeyAsync(key);
    }

    async void FilterKeyBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(ViewModel != null)
            await ViewModel.CommitFilterKeyAsync(args.QueryText);
    }

    async void EditSoftwareBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(ViewModel == null) return;

        ViewModel.UpdateEditSoftwareSuggestions(sender.Text);
        ViewModel.SyncEditSoftwareSelection();

        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && string.IsNullOrWhiteSpace(sender.Text))
            await ViewModel.SelectEditSoftwareAsync(null);
    }

    async void EditSoftwareBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(ViewModel != null && args.SelectedItem is SoftwareDto software)
            await ViewModel.SelectEditSoftwareAsync(software);
    }

    async void EditSoftwareBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(ViewModel == null) return;

        if(args.ChosenSuggestion is SoftwareDto software)
        {
            await ViewModel.SelectEditSoftwareAsync(software);

            return;
        }

        await ViewModel.ResolveEditSoftwareQueryAsync(args.QueryText);
    }

    async void EditCategoryBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(ViewModel != null && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            await ViewModel.UpdateEditCategorySuggestionsAsync(sender.Text);
    }

    async void EditCategoryBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(ViewModel != null && args.SelectedItem is string category)
        {
            ViewModel.EditCategory = category;
            await ViewModel.UpdateEditKeySuggestionsAsync(ViewModel.EditKey, category);
        }
    }

    async void EditCategoryBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(ViewModel != null)
        {
            ViewModel.EditCategory = args.QueryText;
            await ViewModel.UpdateEditKeySuggestionsAsync(ViewModel.EditKey, args.QueryText);
        }
    }

    async void EditKeyBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(ViewModel != null && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            await ViewModel.UpdateEditKeySuggestionsAsync(sender.Text);
    }

    void EditKeyBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(ViewModel != null && args.SelectedItem is string key)
            ViewModel.EditKey = key;
    }

    void EditKeyBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(ViewModel != null)
            ViewModel.EditKey = args.QueryText;
    }

    async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(ViewModel == null) return;

        ViewModel.CurrentPage = 1;
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
