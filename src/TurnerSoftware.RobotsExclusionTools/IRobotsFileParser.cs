using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TurnerSoftware.RobotsExclusionTools
{
	public interface IRobotsFileParser
	{
		/// <summary>
		/// Creates a <see cref="RobotsFile"/> for the provided <paramref name="robotsText"/> and using the <paramref name="baseUri"/>.
		/// </summary>
		/// <param name="robotsText"></param>
		/// <param name="baseUri"></param>
		/// <returns></returns>
		RobotsFile FromString(string robotsText, Uri baseUri);
		/// <summary>
		/// Creates a <see cref="RobotsFile"/> for the provided <paramref name="robotsUri"/> and using the <see cref="RobotsFileAccessRules.NoRobotsRfc"/> access rules.
		/// </summary>
		/// <param name="robotsUri"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<RobotsFile> FromUriAsync(Uri robotsUri, CancellationToken cancellationToken = default);
		/// <summary>
		/// Creates a <see cref="RobotsFile"/> for the provided <paramref name="robotsUri"/> and using the specified <paramref name="accessRules"/>.
		/// </summary>
		/// <param name="robotsUri"></param>
		/// <param name="accessRules"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<RobotsFile> FromUriAsync(Uri robotsUri, RobotsFileAccessRules robotsFileAccessRules, CancellationToken cancellationToken = default);
		/// <summary>
		/// Creates a <see cref="RobotsFile"/> from the data in the <paramref name="stream"/> and using the <paramref name="baseUri"/>.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="baseUri"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<RobotsFile> FromStreamAsync(Stream stream, Uri baseUri, CancellationToken cancellationToken = default);
	}
}
