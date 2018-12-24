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

			//Blank "Disallow" means no restriction
			if (accessEntry.Disallow.Any(d => d == string.Empty))
			{
				return true;
			}

			if (accessEntry.Disallow.Any(d => PathMatch(d, requestPath, StringComparison.InvariantCulture)))
			{
				if (accessEntry.Allow.Any(a => PathMatch(a, requestPath, StringComparison.InvariantCulture)))
				{
					return true;
				}
				else
				{
					return false;
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

	public class PathComparisonResult
	{
		public string MatchedOn { get; set; }
		public bool RuleResult { get; set; }
	}
}
