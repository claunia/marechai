using System;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using static Marechai.App.Presentation.Views.RichTextHelpers;

namespace Marechai.App.Presentation.Views;

public sealed partial class ContactPage : Page
{
    public ContactPage()
    {
        InitializeComponent();
        DataContextChanged += ContactPage_DataContextChanged;
    }

    private void ContactPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is ContactViewModel viewModel) RenderContent(viewModel);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is ContactViewModel viewModel) RenderContent(viewModel);
    }

    private void RenderContent(ContactViewModel viewModel)
    {
        IStringLocalizer l = viewModel.Localizer;

        ContentPanel.Children.Clear();
        ContentPanel.Children.Add(BuildContactSection(l, viewModel));
    }

    private static Border BuildContactSection(IStringLocalizer l, ContactViewModel viewModel)
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(BuildHeaderTextBlock(l["AboutPage_Contact_Header"].Value));
        stack.Children.Add(BuildRichTextBlock(l["AboutPage_Contact_Body"].Value));

        var textBlock = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap };
        textBlock.Inlines.Add(new Run { Text = $"{l["AboutPage_Support"].Value}: ", FontWeight = FontWeights.Bold });

        var hyperlink = new Hyperlink();
        hyperlink.Inlines.Add(new Run { Text = "museum@claunia.com" });
        hyperlink.Click += (_, _) => _ = Launcher.LaunchUriAsync(new Uri("mailto:museum@claunia.com"));
        textBlock.Inlines.Add(hyperlink);
        stack.Children.Add(textBlock);

        var aboutButton = new Button
        {
            Content = l["AboutMarechaiButton"].Value,
            Margin  = new Thickness(0, 8, 0, 0),
            Command = viewModel.NavigateToAboutCommand
        };

        stack.Children.Add(aboutButton);

        return WrapInCard(stack);
    }
}
