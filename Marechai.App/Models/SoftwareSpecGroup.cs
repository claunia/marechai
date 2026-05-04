using System.Collections.ObjectModel;

namespace Marechai.App.Models;

public class SoftwareSpecGroup
{
    public required string                                     Key        { get; set; }
    public required string                                     DisplayKey { get; set; }
    public required ObservableCollection<SoftwareSpecValueItem> Values     { get; set; }
}

public class SoftwareSpecValueItem
{
    public required string Key          { get; set; }
    public required string Value        { get; set; }
    public required string DisplayValue { get; set; }
}
