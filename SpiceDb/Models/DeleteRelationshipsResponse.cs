using SpiceDb.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Models;

/// <summary>
/// Represents the result of a delete relationships operation.
/// Contains the deletion token and an indicator of whether the deletion was complete or partial.
/// </summary>
public class DeleteRelationshipsResponse
{
	/// <summary>
	/// Gets or sets the token representing the state of the system at which the relationships were deleted.
	/// </summary>
	public ZedToken? DeletedAt { get; set; }

	/// <summary>
	/// Gets or sets the progress of the deletion operation.
	/// Maps to the DeletionProgress enum in the protobuf.
	/// </summary>
	public DeletionProgress DeletionProgress { get; set; } = DeletionProgress.Unspecified;
}
