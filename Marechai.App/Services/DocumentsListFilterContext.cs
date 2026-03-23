namespace Marechai.App.Services;

public interface IDocumentsListFilterContext
{
    DocumentListFilterType FilterType  { get; set; }
    string                 FilterValue { get; set; }
}

public class DocumentsListFilterContext : IDocumentsListFilterContext
{
    public DocumentListFilterType FilterType  { get; set; } = DocumentListFilterType.All;
    public string                 FilterValue { get; set; } = string.Empty;
}

public enum DocumentListFilterType
{
    All,
    Letter,
    Year
}
