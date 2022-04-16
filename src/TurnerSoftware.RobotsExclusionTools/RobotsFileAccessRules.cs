namespace TurnerSoftware.RobotsExclusionTools;

public struct RobotsFileAccessRules
{
	/// <summary>
	/// Controls whether a 401 "Unauthorized" error while accessing the Robots.txt file means unrestricted access to the site.
	/// </summary>
	public bool AllowAllWhen401Unauthorized { get; set; }
	/// <summary>
	/// Controls whether a 403 "Forbidden" error while accessing the Robots.txt file means unrestricted access to the site.
	/// </summary>
	public bool AllowAllWhen403Forbidden { get; set; }
	/// <summary>
	/// Controls whether a 404 "Not Found" while accessing the Robots.txt file means unrestricted access to the site.
	/// </summary>
	public bool AllowAllWhen404NotFound { get; set; }

	/// <summary>
	/// Based on the <a href="http://www.robotstxt.org/norobots-rfc.txt">NoRobots RFC rules (section 3.1)</a>
	/// </summary>
	public static readonly RobotsFileAccessRules NoRobotsRfc = new()
	{
		AllowAllWhen404NotFound = true
	};

	/// <summary>
	/// Based on the <a href="https://developers.google.com/search/docs/advanced/robots/robots_txt#handling-http-result-codes">rules used by Google</a>.
	/// </summary>
	/// <remarks>
	/// Requests that return 401 or 403 allow unrestricted access.
	/// Like the NoRobots RFC, 404 errors are also unrestricted.
	/// </remarks>
	public static readonly RobotsFileAccessRules LikeGoogle = new()
	{
		AllowAllWhen401Unauthorized = true,
		AllowAllWhen403Forbidden = true,
		AllowAllWhen404NotFound = true
	};
}
