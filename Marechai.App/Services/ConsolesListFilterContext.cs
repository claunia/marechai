namespace Marechai.App.Services;

/// <summary>
///     Context for filtering consoles by various criteria
/// </summary>
public interface IConsolesListFilterContext
{
    ConsoleListFilterType FilterType  { get; set; }
    string                FilterValue { get; set; }
}

/// <summary>
///     Implementation of the consoles list filter context
/// </summary>
public class ConsolesListFilterContext : IConsolesListFilterContext
{
    public ConsoleListFilterType FilterType  { get; set; } = ConsoleListFilterType.All;
    public string                FilterValue { get; set; } = string.Empty;
}

/// <summary>
///     Enumeration for console list filter types
/// </summary>
public enum ConsoleListFilterType
{
    All,
    Letter,
    Year
}