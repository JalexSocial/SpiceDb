using SpiceDb.Enum;

namespace SpiceDb.Models;

public class AlgebraicSubjectSet
{
    public AlgebraicSubjectSetOperation Operation { get; set; } = AlgebraicSubjectSetOperation.Unspecified;
    public List<PermissionRelationshipTree> Children { get; set; } = new();
}
