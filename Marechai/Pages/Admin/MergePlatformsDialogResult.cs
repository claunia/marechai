using System.Collections.Generic;

namespace Marechai.Pages.Admin;

public class MergePlatformsDialogResult
{
    public ulong TargetId { get; set; }
    public List<ulong> SourceIds { get; set; }
}
