namespace Marechai.App.Services;

public interface IBooksListFilterContext
{
    BookListFilterType FilterType  { get; set; }
    string             FilterValue { get; set; }
}

public class BooksListFilterContext : IBooksListFilterContext
{
    public BookListFilterType FilterType  { get; set; } = BookListFilterType.All;
    public string             FilterValue { get; set; } = string.Empty;
}

public enum BookListFilterType
{
    All,
    Letter,
    Year
}
