namespace Marechai.App.Models;

public class SoftwareListItem
{
    public int     Id                { get; set; }
    public string  Name              { get; set; } = string.Empty;
    public string? Family            { get; set; }
    public int?    Year              { get; set; }
    public bool    IsOperatingSystem { get; set; }
    public bool    IsGame            { get; set; }
}
