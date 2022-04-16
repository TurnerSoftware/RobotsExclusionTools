namespace TurnerSoftware.RobotsExclusionTools;

public static class RobotsPageDirectives
{
	public const string All = "all";
	public const string NoIndex = "noindex";
	public const string NoFollow = "nofollow";
	public const string None = "none";
	public const string NoArchive = "noarchive";
	public const string NoSitelinksSearchBox = "nositelinkssearchbox";
	public const string NoSnippet = "nosnippet";
	public const string IndexIfEmbedded = "indexifembedded";
	public const string MaxSnippet = "max-snippet";
	public const string MaxImagePreview = "max-image-preview";
	public const string MaxVideoPreview = "max-video-preview";
	public const string NoTranslate = "notranslate";
	public const string NoImageIndex = "noimageindex";
	public const string UnavailableAfter = "unavailable_after";

	public static DirectiveType GetDirectiveType(string value) => value switch
	{
		All => DirectiveType.ValueOnly,
		NoIndex => DirectiveType.ValueOnly,
		NoFollow => DirectiveType.ValueOnly,
		None => DirectiveType.ValueOnly,
		NoArchive => DirectiveType.ValueOnly,
		NoSitelinksSearchBox => DirectiveType.ValueOnly,
		NoSnippet => DirectiveType.ValueOnly,
		IndexIfEmbedded => DirectiveType.ValueOnly,
		MaxSnippet => DirectiveType.FieldWithValue,
		MaxImagePreview => DirectiveType.FieldWithValue,
		MaxVideoPreview => DirectiveType.FieldWithValue,
		NoTranslate => DirectiveType.ValueOnly,
		NoImageIndex => DirectiveType.ValueOnly,
		UnavailableAfter => DirectiveType.FieldWithValue,
		_ => DirectiveType.Unknown
	};

	public enum DirectiveType
	{
		Unknown,
		ValueOnly,
		FieldWithValue
	}
}