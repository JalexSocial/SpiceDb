namespace SpiceDb.Models;

/// <summary>
/// Represents a cursor used for pagination in streaming responses.
/// Corresponds to the Cursor message defined in the protobuf.
/// </summary>
public class Cursor
{
	/// <summary>
	/// Gets or sets the token representing the current cursor position.
	/// </summary>
	public string Token { get; set; } = string.Empty;
}