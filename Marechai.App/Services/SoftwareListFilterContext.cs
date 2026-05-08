using Marechai.Data;

namespace Marechai.App.Services;

public interface ISoftwareListFilterContext
{
    SoftwareListFilterType FilterType  { get; set; }
    string                 FilterValue { get; set; }
    SoftwareKind?          Kind        { get; set; }
}

public class SoftwareListFilterContext : ISoftwareListFilterContext
{
    public SoftwareListFilterType FilterType  { get; set; } = SoftwareListFilterType.All;
    public string                 FilterValue { get; set; } = string.Empty;
    public SoftwareKind?          Kind        { get; set; }
}

public enum SoftwareListFilterType
{
    All,
    Letter,
    Year,
    Platform,
    Spec
}
