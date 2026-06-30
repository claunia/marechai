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

    public static readonly DependencyProperty HeroScoreValueProperty =
        DependencyProperty.Register(nameof(HeroScoreValue), typeof(double?), typeof(HeroHeader),
                                     new PropertyMetadata(null, OnHeroScoreValueChanged));

    public static readonly DependencyProperty HeroScoreTextProperty =
        DependencyProperty.Register(nameof(HeroScoreText), typeof(string), typeof(HeroHeader),
                                     new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeroScoreLabelProperty =
        DependencyProperty.Register(nameof(HeroScoreLabel), typeof(string), typeof(HeroHeader),
                                     new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeroScoreRankTextProperty =
        DependencyProperty.Register(nameof(HeroScoreRankText), typeof(string), typeof(HeroHeader),
                                     new PropertyMetadata(string.Empty, OnHeroScoreRankTextChanged));

    public static readonly DependencyProperty ShowScoreProperty =
        DependencyProperty.Register(nameof(ShowScore), typeof(bool), typeof(HeroHeader),
                                     new PropertyMetadata(false));

    public static readonly DependencyProperty HasScoreRankTextProperty =
        DependencyProperty.Register(nameof(HasScoreRankText), typeof(bool), typeof(HeroHeader),
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

    public double? HeroScoreValue
    {
        get => (double?)GetValue(HeroScoreValueProperty);
        set => SetValue(HeroScoreValueProperty, value);
    }

    public string HeroScoreText
    {
        get => (string)GetValue(HeroScoreTextProperty);
        set => SetValue(HeroScoreTextProperty, value);
    }

    public string HeroScoreLabel
    {
        get => (string)GetValue(HeroScoreLabelProperty);
        set => SetValue(HeroScoreLabelProperty, value);
    }

    public string HeroScoreRankText
    {
        get => (string)GetValue(HeroScoreRankTextProperty);
        set => SetValue(HeroScoreRankTextProperty, value);
    }

    public bool ShowScore
    {
        get => (bool)GetValue(ShowScoreProperty);
        set => SetValue(ShowScoreProperty, value);
    }

    public bool HasScoreRankText
    {
        get => (bool)GetValue(HasScoreRankTextProperty);
        set => SetValue(HasScoreRankTextProperty, value);
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var header = (HeroHeader)d;
        header.HasSubtitle = !string.IsNullOrWhiteSpace(e.NewValue as string);
    }

    private static void OnHeroScoreValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var header = (HeroHeader)d;
        header.ShowScore = e.NewValue is double;
    }

    private static void OnHeroScoreRankTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var header = (HeroHeader)d;
        header.HasScoreRankText = !string.IsNullOrWhiteSpace(e.NewValue as string);
    }
}
