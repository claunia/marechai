using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public abstract class DocumentBaseDto : BaseDto<long>
{
    [JsonPropertyName("title")]
    [Required]
    public required string Title { get; set; }
    [JsonPropertyName("native_title")]
    public string? NativeTitle { get; set; }
    [JsonPropertyName("published")]
    public DateTime? Published { get; set; }
    [JsonPropertyName("country_id")]
    public short? CountryId { get; set; }
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("synopsis")]
    public string? Synopsis { get; set; }
}