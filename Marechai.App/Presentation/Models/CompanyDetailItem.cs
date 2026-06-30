namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a related entity (machine family, GPU, processor, sound synth, software,
///     book, document, magazine or person) shown in the company detail view
/// </summary>
public class CompanyDetailItem
{
    public long   Id   { get; set; }
    public string Name { get; set; } = string.Empty;
}
