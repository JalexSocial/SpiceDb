namespace SpiceDb.Models;

/// <summary>
/// Represents a token used to denote a point-in-time or a causality marker for permission operations.
/// Corresponds to the ZedToken message in the core.proto.
/// </summary>
public class ZedToken
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ZedToken"/> class.
	/// </summary>
	/// <param name="token">The token string.</param>
	public ZedToken(string? token)
	{
		Token = token ?? string.Empty;
	}

	/// <summary>
	/// Gets or sets the token string.
	/// </summary>
	public string Token { get; set; }
}