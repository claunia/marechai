using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class CoverDetailPage : Page
{
    const double ZoomFactor = 1.5;

    public CoverDetailPage()
    {
        InitializeComponent();
    }

    void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        double currentW = double.IsNaN(CoverImage.Width) ? CoverImage.ActualWidth : CoverImage.Width;
        double currentH = double.IsNaN(CoverImage.Height) ? CoverImage.ActualHeight : CoverImage.Height;

        if(currentW <= 0 || currentH <= 0) return;

        CoverImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
        CoverImage.Width   = currentW * ZoomFactor;
        CoverImage.Height  = currentH * ZoomFactor;
    }

    void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        double currentW = double.IsNaN(CoverImage.Width) ? CoverImage.ActualWidth : CoverImage.Width;
        double currentH = double.IsNaN(CoverImage.Height) ? CoverImage.ActualHeight : CoverImage.Height;

        if(currentW <= 0 || currentH <= 0) return;

        CoverImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
        CoverImage.Width   = currentW / ZoomFactor;
        CoverImage.Height  = currentH / ZoomFactor;
    }

    void ZoomReset_Click(object sender, RoutedEventArgs e)
    {
        CoverImage.Width   = double.NaN;
        CoverImage.Height  = double.NaN;
        CoverImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform;
    }
}
