namespace SpiceDb.Models;

/// <summary>
/// Represents a status message typically used to convey error information.
/// Contains an error code, a developer-facing message, and additional details.
/// This is similar to the google.rpc.Status message used in the protobuf.
/// </summary>
public class Status
{
	/// <summary>
	/// Gets or sets the error code.
	/// </summary>
	public int Code { get; set; }

	/// <summary>
	/// Gets or sets the error message.
	/// </summary>
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets any additional details regarding the error.
	/// </summary>
	public List<object> Details { get; set; } = new List<object>();
}