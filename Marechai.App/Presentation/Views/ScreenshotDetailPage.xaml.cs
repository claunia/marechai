using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class ScreenshotDetailPage : Page
{
    const double ZoomFactor = 1.5;

    public ScreenshotDetailPage()
    {
        InitializeComponent();
    }

    void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        double currentW = double.IsNaN(ScreenshotImage.Width) ? ScreenshotImage.ActualWidth : ScreenshotImage.Width;
        double currentH = double.IsNaN(ScreenshotImage.Height) ? ScreenshotImage.ActualHeight : ScreenshotImage.Height;

        if(currentW <= 0 || currentH <= 0) return;

        ScreenshotImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
        ScreenshotImage.Width   = currentW * ZoomFactor;
        ScreenshotImage.Height  = currentH * ZoomFactor;
    }

    void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        double currentW = double.IsNaN(ScreenshotImage.Width) ? ScreenshotImage.ActualWidth : ScreenshotImage.Width;
        double currentH = double.IsNaN(ScreenshotImage.Height) ? ScreenshotImage.ActualHeight : ScreenshotImage.Height;

        if(currentW <= 0 || currentH <= 0) return;

        ScreenshotImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
        ScreenshotImage.Width   = currentW / ZoomFactor;
        ScreenshotImage.Height  = currentH / ZoomFactor;
    }

    void ZoomReset_Click(object sender, RoutedEventArgs e)
    {
        ScreenshotImage.Width   = double.NaN;
        ScreenshotImage.Height  = double.NaN;
        ScreenshotImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform;
    }
}
