namespace SpiceDb.Models;

public class Permission : Relationship
{
    public Permission(ResourceReference resource, string relation, ResourceReference subject) : base(resource, relation, subject)
    { }

    public Permission(string resource, string relation, string subject) : base(resource, relation, subject)
    {
    }

    /// <summary>
    /// Creates a permission based on the format resource#relation@subject
    /// Example: user:1234#view@user:1 (can user:1 view user:1234?)
    /// </summary>
    /// <param name="permission"></param>
    /// <exception cref="ArgumentException"></exception>
    public Permission(string permission) : base(permission)
    {
    }
}
