using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Hosting;

namespace Marechai.App.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();
    }

    public ContentControl ContentControl => Splash;
}
