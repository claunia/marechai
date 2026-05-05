using System;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

public partial class MachinePhotoDisplayItem : ObservableObject
{
    public Guid    PhotoId     { get; set; }
    public string LicenseName { get; set; }
    public string CameraInfo  { get; set; }
    public string UploadDate  { get; set; }

    [ObservableProperty]
    private ImageSource _thumbnailImageSource;
}
