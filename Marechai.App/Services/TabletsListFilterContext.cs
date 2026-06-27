namespace Marechai.App.Services;

/// <summary>
///     Context for filtering Tablets by various criteria
/// </summary>
public interface ITabletsListFilterContext
{
    TabletListFilterType FilterType  { get; set; }
    string                FilterValue { get; set; }
}

/// <summary>
///     Implementation of the Tablets list filter context
/// </summary>
public class TabletsListFilterContext : ITabletsListFilterContext
{
    public TabletListFilterType FilterType  { get; set; } = TabletListFilterType.All;
    public string                FilterValue { get; set; } = string.Empty;
}

/// <summary>
///     Enumeration for Tablet list filter types
/// </summary>
public enum TabletListFilterType
{
    All,
    Letter,
    Year,
    Prototype
}
