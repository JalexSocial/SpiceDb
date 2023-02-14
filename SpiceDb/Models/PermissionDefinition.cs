using Authzed.Api.V1;

namespace SpiceDb.Models;

public class PermissionDefinition
{
    public PermissionDefinition(string name, string definition)
    {
	    Name = name;
        Definition = definition;
    }

    public string Name { get; set; }
    public string Definition { get; set; }
}
