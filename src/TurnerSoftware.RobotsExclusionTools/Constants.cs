using System;

namespace TurnerSoftware.RobotsExclusionTools;

public static class Constants
{
	public const string UserAgentField = "User-agent";

	public const string DisallowField = "Disallow";
	public const string AllowField = "Allow";

	public const string CrawlDelayField = "Crawl-delay";
	public const string SitemapField = "Sitemap";

	public const string UserAgentWildcard = "*";

	public static class UTF8
	{
		public static ReadOnlySpan<byte> UserAgentField => "User-agent"u8;

		public static ReadOnlySpan<byte> DisallowField => "Disallow"u8;
		public static ReadOnlySpan<byte> AllowField => "Allow"u8;

		public static ReadOnlySpan<byte> CrawlDelayField => "Crawl-delay"u8;
		public static ReadOnlySpan<byte> SitemapField => "Sitemap"u8;
	}
}