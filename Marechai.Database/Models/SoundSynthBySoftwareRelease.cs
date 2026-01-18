namespace Marechai.Database.Models;

public class SoundSynthBySoftwareRelease
{
    public         ulong           ReleaseId { get; set; }
    public virtual SoftwareRelease Release   { get; set; }

    public         int        SoundSynthId { get; set; }
    public virtual SoundSynth SoundSynth   { get; set; }
}