#nullable enable

using System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Marechai.App.Presentation.Views;

public sealed partial class MessageMarkdownView : UserControl
{
    public static readonly DependencyProperty MarkdownProperty =
        DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MessageMarkdownView),
            new PropertyMetadata(string.Empty, OnMarkdownChanged));

    public string Markdown
    {
        get => (string)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    public MessageMarkdownView()
    {
        InitializeComponent();
    }

    static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(d is MessageMarkdownView view)
            view.MarkdownBlock.Text = e.NewValue as string ?? string.Empty;
    }

    async void MarkdownBlock_OnLinkClicked(object sender, LinkClickedEventArgs e)
    {
        if(string.IsNullOrWhiteSpace(e.Link)) return;

        if(Uri.TryCreate(e.Link, UriKind.Absolute, out Uri? uri) &&
           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeMailto))
            await Launcher.LaunchUriAsync(uri);
    }
}
