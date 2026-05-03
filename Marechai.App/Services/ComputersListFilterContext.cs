namespace Marechai.App.Services;

/// <summary>
///     Service to hold the current filter context for the computers list view
/// </summary>
public interface IComputersListFilterContext
{
    ComputerListFilterType FilterType  { get; set; }
    string                 FilterValue { get; set; }
}

/// <summary>
///     Implementation of the computers list filter context
/// </summary>
public class ComputersListFilterContext : IComputersListFilterContext
{
    public ComputerListFilterType FilterType  { get; set; } = ComputerListFilterType.All;
    public string                 FilterValue { get; set; } = string.Empty;
}

/// <summary>
///     Enum for computer filter types
/// </summary>
public enum ComputerListFilterType
{
    All,
    Letter,
    Year,
    Prototype
}