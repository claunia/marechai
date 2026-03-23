namespace Marechai.App.Services;

public interface IPeopleListFilterContext
{
    PeopleListFilterType FilterType  { get; set; }
    string               FilterValue { get; set; }
}

public class PeopleListFilterContext : IPeopleListFilterContext
{
    public PeopleListFilterType FilterType  { get; set; } = PeopleListFilterType.All;
    public string               FilterValue { get; set; } = string.Empty;
}

public enum PeopleListFilterType
{
    All,
    Letter,
    Year
}
