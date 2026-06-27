using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class SoftwarePromoArtDetailPage : Page
{
    const double ZoomFactor = 1.5;

    public SoftwarePromoArtDetailPage()
    {
        InitializeComponent();
    }

    void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        double currentW = double.IsNaN(PromoArtImage.Width) ? PromoArtImage.ActualWidth : PromoArtImage.Width;
        double currentH = double.IsNaN(PromoArtImage.Height) ? PromoArtImage.ActualHeight : PromoArtImage.Height;

        if(currentW <= 0 || currentH <= 0) return;

        PromoArtImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
        PromoArtImage.Width   = currentW * ZoomFactor;
        PromoArtImage.Height  = currentH * ZoomFactor;
    }

    void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        double currentW = double.IsNaN(PromoArtImage.Width) ? PromoArtImage.ActualWidth : PromoArtImage.Width;
        double currentH = double.IsNaN(PromoArtImage.Height) ? PromoArtImage.ActualHeight : PromoArtImage.Height;

        if(currentW <= 0 || currentH <= 0) return;

        PromoArtImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
        PromoArtImage.Width   = currentW / ZoomFactor;
        PromoArtImage.Height  = currentH / ZoomFactor;
    }

    void ZoomReset_Click(object sender, RoutedEventArgs e)
    {
        PromoArtImage.Width   = double.NaN;
        PromoArtImage.Height  = double.NaN;
        PromoArtImage.Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform;
    }
}
