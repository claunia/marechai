using System.Collections.Generic;

namespace Marechai.Pages.Admin;

public class MergeSoftwareCompilationsDialogResult
{
    public ulong TargetId { get; set; }
    public List<ulong> SourceIds { get; set; }
}
