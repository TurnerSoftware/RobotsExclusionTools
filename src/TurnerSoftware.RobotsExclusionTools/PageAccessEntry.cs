using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class PageAccessEntry
	{
		public string UserAgent { get; set; }
		
		public IEnumerable<PageAccessRule> Rules { get; set; }
	}
	
	public class PageAccessRule
	{
		public string RuleName { get; set; }
		
		public string RuleValue { get; set; }
	}
}
