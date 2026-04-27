namespace Marechai.Database.Models;

public class GenreBySoftware
{
    public         ulong         SoftwareId { get; set; }
    public virtual Software      Software   { get; set; }
    public         int           GenreId    { get; set; }
    public virtual SoftwareGenre Genre      { get; set; }
}
