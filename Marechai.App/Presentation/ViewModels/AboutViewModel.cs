namespace Marechai.App.Presentation.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public AboutViewModel(IStringLocalizer localizer) => Localizer = localizer;

    public IStringLocalizer Localizer { get; }
}
