using System;
using Marechai.Data;

namespace Marechai.App.Presentation;

public static class DatePrecisionFormatter
{
    public static string? Format(DateTime? date, int? precision, string? empty = "") =>
        date.HasValue ? Format(date.Value, precision) : empty;

    public static string? Format(DateTimeOffset? date, int? precision, string? empty = "") =>
        date.HasValue ? Format(date.Value.DateTime, precision) : empty;

    public static string Format(DateTime date, int? precision)
    {
        return (precision ?? 0) switch
        {
            (int)DatePrecision.YearOnly  => $"{date.Year}",
            (int)DatePrecision.MonthYear => date.ToString("MMMM yyyy"),
            _                            => date.ToString("MMMM d, yyyy")
        };
    }

    public static string? FormatWithTrailingPeriod(DateTimeOffset? date, int? precision, string? empty = "")
    {
        string? formatted = Format(date, precision, empty);

        return string.IsNullOrEmpty(formatted) ? formatted : $"{formatted}.";
    }
}
