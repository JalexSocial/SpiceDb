using SpiceDb.Enum;

namespace SpiceDb.Models;

public class ResolvedSubject
{
    public string Id { get; set; } = string.Empty;
    public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;
    public List<string> MissingRequiredContext { get; set; } = new();
}

public class LookupSubjectsResponse
{
    public ZedToken? LookedUpAt { get; set; }
    public ResolvedSubject Subject { get; set; } = new();
    public List<ResolvedSubject> ExcludedSubjects { get; set; } = new();
}
