namespace Marechai.App.Services;

/// <summary>
///     Context for filtering smartphones by various criteria
/// </summary>
public interface ISmartphonesListFilterContext
{
    SmartphoneListFilterType FilterType  { get; set; }
    string                   FilterValue { get; set; }
}

/// <summary>
///     Implementation of the smartphones list filter context
/// </summary>
public class SmartphonesListFilterContext : ISmartphonesListFilterContext
{
    public SmartphoneListFilterType FilterType  { get; set; } = SmartphoneListFilterType.All;
    public string                   FilterValue { get; set; } = string.Empty;
}

/// <summary>
///     Enumeration for smartphone list filter types
/// </summary>
public enum SmartphoneListFilterType
{
    All,
    Letter,
    Year
}
