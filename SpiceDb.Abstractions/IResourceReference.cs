namespace SpiceDb.Abstractions;

public interface IResourceReference
{
    string Type { get; set; }
    string Id { get; set; }
    string Relation { get; set; }

    /// <summary>
    /// Sets up a resource reference that can reference a specific relation/permission inside an object
    /// For example: organization:easd may be an original reference, but to reference members of easd
    /// the reference should be: organization:easd#members 
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    IResourceReference WithSubjectRelation(string relation);

    IResourceReference EnsurePrefix(string prefix);
    IResourceReference ExcludePrefix(string prefix);
    string ToString();
}