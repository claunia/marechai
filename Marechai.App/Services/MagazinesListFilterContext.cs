namespace Marechai.App.Services;

public interface IMagazinesListFilterContext
{
    MagazineListFilterType FilterType  { get; set; }
    string                 FilterValue { get; set; }
}

public class MagazinesListFilterContext : IMagazinesListFilterContext
{
    public MagazineListFilterType FilterType  { get; set; } = MagazineListFilterType.All;
    public string                 FilterValue { get; set; } = string.Empty;
}

public enum MagazineListFilterType
{
    All,
    Letter,
    Year
}
