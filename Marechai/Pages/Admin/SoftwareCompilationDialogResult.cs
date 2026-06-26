namespace Marechai.Pages.Admin;

public sealed class SoftwareCompilationDialogResult
{
    public string                                Name             { get; set; }
    public int?                                  SoftwareId       { get; set; }
    public int?                                  MachineId        { get; set; }
    public int?                                  PredecessorId    { get; set; }
    public Marechai.Data.SoftwareRelationshipType RelationshipType { get; set; }
}
