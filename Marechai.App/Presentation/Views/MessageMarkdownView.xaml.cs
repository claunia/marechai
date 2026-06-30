#nullable enable

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        if(d is not MessageMarkdownView view) return;

        view.RootGrid.Children.Clear();
        view.RootGrid.Children.Add(MarkdownRenderer.Render(e.NewValue as string));
    }
}
