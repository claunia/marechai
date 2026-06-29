using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Components;

public sealed partial class HeroHeader : UserControl
{
    public static readonly DependencyProperty HeroTitleProperty =
        DependencyProperty.Register(nameof(HeroTitle), typeof(string), typeof(HeroHeader),
                                     new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeroSubtitleProperty =
        DependencyProperty.Register(nameof(HeroSubtitle), typeof(string), typeof(HeroHeader),
                                     new PropertyMetadata(string.Empty, OnSubtitleChanged));

    public static readonly DependencyProperty HasSubtitleProperty =
        DependencyProperty.Register(nameof(HasSubtitle), typeof(bool), typeof(HeroHeader),
                                     new PropertyMetadata(false));

    public static readonly DependencyProperty HeroImageSourceProperty =
        DependencyProperty.Register(nameof(HeroImageSource), typeof(object), typeof(HeroHeader),
                                     new PropertyMetadata(null));

    public static readonly DependencyProperty ShowImageProperty =
        DependencyProperty.Register(nameof(ShowImage), typeof(bool), typeof(HeroHeader),
                                     new PropertyMetadata(false));

    public HeroHeader() => InitializeComponent();

    public string HeroTitle
    {
        get => (string)GetValue(HeroTitleProperty);
        set => SetValue(HeroTitleProperty, value);
    }

    public string HeroSubtitle
    {
        get => (string)GetValue(HeroSubtitleProperty);
        set => SetValue(HeroSubtitleProperty, value);
    }

    public bool HasSubtitle
    {
        get => (bool)GetValue(HasSubtitleProperty);
        set => SetValue(HasSubtitleProperty, value);
    }

    public object HeroImageSource
    {
        get => GetValue(HeroImageSourceProperty);
        set => SetValue(HeroImageSourceProperty, value);
    }

    public bool ShowImage
    {
        get => (bool)GetValue(ShowImageProperty);
        set => SetValue(ShowImageProperty, value);
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var header = (HeroHeader)d;
        header.HasSubtitle = !string.IsNullOrWhiteSpace(e.NewValue as string);
    }
}
