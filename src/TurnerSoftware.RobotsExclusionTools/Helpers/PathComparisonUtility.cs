using System;
using System.Runtime.CompilerServices;

namespace TurnerSoftware.RobotsExclusionTools.Helpers
{
	public static class PathComparisonUtility
	{
		private const char PathEndSpecialCharacter = '$';
		private const char WildcardSpecialCharacter = '*';

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool StartsWith(this ReadOnlySpan<char> source, char value) => source.Length > 0 && source[0] == value;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool EndsWith(this ReadOnlySpan<char> source, char value) => source.Length > 0 && source[source.Length - 1] == value;

		public static bool IsAllowed(SiteAccessEntry accessEntry, Uri requestUri)
		{
			var requestPath = requestUri.PathAndQuery;

			//Robots file is always unrestricted
			if (requestUri.LocalPath == "/robots.txt")
			{
				return true;
			}

			//If no entry is defined, the robot is allowed access by default
			if (accessEntry == default)
			{
				return true;
			}

			foreach (var rule in accessEntry.PathRules)
			{
				if (rule.RuleType == PathRuleType.Disallow && rule.Path == string.Empty)
				{
					return true;
				}
				else if (PathMatch(rule.Path, requestPath, StringComparison.InvariantCulture))
				{
					return rule.RuleType == PathRuleType.Allow;
				}
			}

			return true;
		}
		public static bool PathMatch(string sourceRecord, string uriPath, StringComparison comparison)
			=> PathMatch(sourceRecord.AsSpan(), uriPath.AsSpan(), comparison);

		public static bool PathMatch(ReadOnlySpan<char> sourceRecord, ReadOnlySpan<char> uriPath, StringComparison comparison)
		{
			var localSourceRecord = sourceRecord;
			var mustMatchToEnd = false;
			var mustMatchToStart = true;
			
			if (localSourceRecord.EndsWith(PathEndSpecialCharacter))
			{
				localSourceRecord = localSourceRecord.Slice(0, localSourceRecord.Length - 1);
				mustMatchToEnd = !localSourceRecord.EndsWith(WildcardSpecialCharacter);
			}

			if (localSourceRecord.StartsWith(WildcardSpecialCharacter))
			{
				mustMatchToStart = false;
			}

			var offsetPosition = 0;

			var enumerator = new WildcardPathEnumerator(localSourceRecord);
			var lastPiece = ReadOnlySpan<char>.Empty;
			while (enumerator.MoveNext(out var sourcePiece))
			{
				lastPiece = sourcePiece;
				var checkPath = uriPath.Slice(offsetPosition);
				var indexPosition = checkPath.IndexOf(sourcePiece, comparison);

				if (mustMatchToStart && offsetPosition == 0 && indexPosition > 0)
				{
					return false;
				}
				else if (indexPosition == -1)
				{
					return false;
				}

				offsetPosition += indexPosition + sourcePiece.Length;
			}

			if (mustMatchToEnd)
			{
				return uriPath.EndsWith(lastPiece, comparison);
			}

			return true;
		}

		private ref struct WildcardPathEnumerator
		{
			private readonly ReadOnlySpan<char> path;
			private int index;

			public WildcardPathEnumerator(ReadOnlySpan<char> path)
			{
				this.path = path;
				index = 0;
			}

			public bool MoveNext(out ReadOnlySpan<char> value)
			{
				while (true)
				{
					if (index == path.Length)
					{
						value = default;
						return false;
					}

					var offsetPath = path.Slice(index);
					var nextOffsetIndex = offsetPath.IndexOf(WildcardSpecialCharacter);

					if (nextOffsetIndex == -1)
					{
						value = offsetPath;
						index += offsetPath.Length;
						return true;
					}
					else if (nextOffsetIndex == 0)
					{
						index++;
						continue;
					}

					value = offsetPath.Slice(0, nextOffsetIndex);
					index += nextOffsetIndex + 1;
					return true;
				}
			}
		}
	}
}
