using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Hosting;

namespace Marechai.App.Presentation.Views;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        InitializeComponent();
    }

    public ContentControl ContentControl => Splash;
}