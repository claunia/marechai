namespace Marechai.App.Services;

/// <summary>
///     Context for filtering PDAs by various criteria
/// </summary>
public interface IPdasListFilterContext
{
    PdaListFilterType FilterType  { get; set; }
    string            FilterValue { get; set; }
}

/// <summary>
///     Implementation of the PDAs list filter context
/// </summary>
public class PdasListFilterContext : IPdasListFilterContext
{
    public PdaListFilterType FilterType  { get; set; } = PdaListFilterType.All;
    public string            FilterValue { get; set; } = string.Empty;
}

/// <summary>
///     Enumeration for PDA list filter types
/// </summary>
public enum PdaListFilterType
{
    All,
    Letter,
    Year,
    Prototype
}
