using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public interface IRobotsPageDefinitionParser
	{
		RobotsPageDefinition FromRules(IEnumerable<string> rules);
	}
}
