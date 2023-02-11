using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceDb.Models;

namespace SpiceDb.Example.MyObjects;

// This class just has some syntactic sugar stuff you can implement for each of your object definitions
//ZedDocument.WithId("abc").CanRead(ZedUser.WithId("cat"))
public class ZedDocument : ResourceReference
{
	public ZedDocument(string id) : base($"arch/document:{id}")
	{
	}

	// Create Document object
	public static ZedDocument WithId (string id) => new ZedDocument(id);
}

public class ZedUser : ResourceReference
{
	public ZedUser(string id) : base($"arch/user:{id}")
	{
	}

	// Create Document object
	public static ZedUser WithId(string id) => new ZedUser(id);

	// Define relationship to other objects (note that the "reader" relation is originally defined in the Document object,
	// but we ensure we reference a ZedDocument object that represents the Document in the parameter
	public Permission CanRead(ZedDocument resource) => new Permission(resource, "reader", this);
}

