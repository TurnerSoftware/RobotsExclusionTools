using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TurnerSoftware.RobotsExclusionTools.Tests.TestSite.Controllers
{
	[Route("/")]
	public class RobotsController : ControllerBase
	{
		private SiteContext Context { get; }

		public RobotsController(SiteContext context)
		{
			Context = context;
		}

		[Route("robots.txt")]
		public IActionResult RobotsWithError()
		{
			return StatusCode(Context.StatusCode);
		}
	}
}
