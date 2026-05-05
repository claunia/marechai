using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Views;

public sealed partial class PhotoDetailPage : Page
{
    private const double ZoomFactor = 1.5;

    public PhotoDetailPage()
    {
        InitializeComponent();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        if(FindChildImage(sender as DependencyObject) is { } img)
        {
            double currentW = double.IsNaN(img.Width) ? img.ActualWidth : img.Width;
            double currentH = double.IsNaN(img.Height) ? img.ActualHeight : img.Height;

            if(currentW <= 0 || currentH <= 0)
                return;

            img.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
            img.Width   = currentW * ZoomFactor;
            img.Height  = currentH * ZoomFactor;
        }
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        if(FindChildImage(sender as DependencyObject) is { } img)
        {
            double currentW = double.IsNaN(img.Width) ? img.ActualWidth : img.Width;
            double currentH = double.IsNaN(img.Height) ? img.ActualHeight : img.Height;

            if(currentW <= 0 || currentH <= 0)
                return;

            img.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
            img.Width   = currentW / ZoomFactor;
            img.Height  = currentH / ZoomFactor;
        }
    }

    private void ZoomReset_Click(object sender, RoutedEventArgs e)
    {
        if(FindChildImage(sender as DependencyObject) is { } img)
        {
            img.Width   = double.NaN;
            img.Height  = double.NaN;
            img.Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform;
        }
    }

    /// <summary>
    ///     Walks up the visual tree then searches descendants for an Image element.
    /// </summary>
    private static Image FindChildImage(DependencyObject element)
    {
        while(element is not null)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            if(parent is not null)
            {
                Image img = FindImage(parent);

                if(img is not null)
                    return img;
            }

            element = parent;
        }

        return null;
    }

    private static Image FindImage(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);

        for(int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            if(child is Image img)
                return img;

            Image result = FindImage(child);

            if(result is not null)
                return result;
        }

        return null;
    }
}