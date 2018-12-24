using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TurnerSoftware.RobotsExclusionTools
{
	public interface IRobotsParser
	{
		RobotsFile FromString(string robotsText, Uri baseUri);
		Task<RobotsFile> FromUriAsync(Uri robotsUri);
		Task<RobotsFile> FromStreamAsync(Stream stream, Uri baseUri);
	}
}
