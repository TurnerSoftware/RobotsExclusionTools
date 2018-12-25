using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class PathComparisonUtility
	{
		public bool IsAllowed(SiteAccessEntry accessEntry, Uri requestUri)
		{
			var requestPath = requestUri.PathAndQuery;

			//Robots file is always unrestricted
			if (requestUri.LocalPath == "/robots.txt")
			{
				return true;
			}

			//If no entry is defined, the robot is allowed access by default
			if (accessEntry == null)
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

		public bool PathMatch(string sourceRecord, string uriPath, StringComparison comparison)
		{
			var sourcePieces = sourceRecord.Split(new[] { '*' }).ToArray();
			var lastPiece = sourcePieces.LastOrDefault();
			var mustMatchToEnd = false;

			if (lastPiece.EndsWith("$"))
			{
				//Remove the last dollar sign from the last piece
				lastPiece = lastPiece.Substring(lastPiece.Length - 1);
				sourcePieces[sourcePieces.Length - 1] = lastPiece;
				mustMatchToEnd = true;
			}

			var offsetPosition = 0;

			foreach (var piece in sourcePieces)
			{
				var indexPosition = uriPath.IndexOf(piece, offsetPosition, comparison);

				if ((offsetPosition == 0 && indexPosition == 0) || indexPosition >= (offsetPosition + piece.Length))
				{
					offsetPosition = indexPosition + 1;
				}
				else
				{
					return false;
				}
			}

			if (mustMatchToEnd)
			{
				var endOffset = uriPath.Length - lastPiece.Length;
				if (uriPath.IndexOf(lastPiece, endOffset, comparison) == -1)
				{
					return false;
				}
			}

			return true;
		}
	}
}
