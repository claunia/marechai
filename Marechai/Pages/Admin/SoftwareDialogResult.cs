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

    /// <summary>
    ///     Company/role junction pairs queued during CREATE-mode editing of
    ///     <see cref="SoftwareDialog" />. Empty in EDIT mode (the dialog persists
    ///     Add/Remove immediately to the server). The parent admin page flushes each
    ///     pair via <c>SoftwareService.AddCompanyRoleAsync</c> after <c>CreateAsync</c>
    ///     returns the new software id. <c>SoftwareCompanyRole</c> has a 3-column
    ///     composite primary key (SoftwareId + CompanyId + RoleId), so the same
    ///     company under a different role is a distinct pair.
    /// </summary>
    public List<PendingCompanyRole> PendingCompanyRoles { get; set; } = [];
}

/// <summary>
///     Buffered company/role junction pick from the admin "New Software" dialog.
///     <paramref name="RoleId" /> is the 3-char ASCII role code (e.g. "dev", "pub").
/// </summary>
public sealed record PendingCompanyRole(int CompanyId, string RoleId);
