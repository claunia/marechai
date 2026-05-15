using System.Collections.Generic;
using Marechai.Data;

namespace Marechai.Pages.Admin;

public sealed class SoftwareDialogResult
{
    public string       Name            { get; set; } = null!;
    public int?         FamilyId        { get; set; }
    public int?         PredecessorId   { get; set; }
    public int?         BaseSoftwareId  { get; set; }
    public SoftwareKind Kind            { get; set; }

    /// <summary>
    ///     Genre ids queued during CREATE-mode editing of <see cref="SoftwareDialog" />.
    ///     Empty in EDIT mode (the dialog persists Add/Remove immediately to the server).
    ///     The parent admin page flushes these via <c>SoftwareService.AddGenreLinkAsync</c>
    ///     after <c>CreateAsync</c> returns the new software id.
    /// </summary>
    public List<int> PendingGenreIds { get; set; } = [];
}
