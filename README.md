<div align="center">

![Icon](images/icon.png)
# Robots Exclusion Tools
A "robots.txt" parsing and querying library for .NET

Closely following the [NoRobots RFC](http://www.robotstxt.org/norobots-rfc.txt), [Robots Exclusion Protocol RFC](https://datatracker.ietf.org/doc/html/draft-koster-rep) and other details on [robotstxt.org](http://www.robotstxt.org/robotstxt.html).

![Build](https://img.shields.io/github/workflow/status/TurnerSoftware/robotsexclusiontools/Build)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/robotsexclusiontools/main.svg)](https://codecov.io/gh/TurnerSoftware/RobotsExclusionTools)
[![NuGet](https://img.shields.io/nuget/v/TurnerSoftware.RobotsExclusionTools.svg)](https://www.nuget.org/packages/TurnerSoftware.RobotsExclusionTools)
</div>

## 📋 Features
- Load Robots by string, by URI (Async) or by streams (Async)
- Supports multiple user-agents and wildcard user-agent (`*`)
- Supports `Allow` and `Disallow`
- Supports `Crawl-delay` entries
- Supports `Sitemap` entries
- Supports wildcard paths (`*`) as well as must-end-with declarations (`$`)
- Dedicated parser for the data from `<meta name="robots" />` tag and the `X-Robots-Tag` header

## 🤝 Licensing and Support

Robots Exclusion Tools is licensed under the MIT license. It is free to use in personal and commercial projects.

There are [support plans](https://turnersoftware.com.au/support-plans) available that cover all active [Turner Software OSS projects](https://github.com/TurnerSoftware).
Support plans provide private email support, expert usage advice for our projects, priority bug fixes and more.
These support plans help fund our OSS commitments to provide better software for everyone.

## NoRobots RFC vs Robots Exclusion Protocol RFC

The [NoRobots RFC](https://www.robotstxt.org/norobots-rfc.txt) was released in 1996 and describes the core syntax that makes up a typical Robots.txt file.
There is a new standard being proposed called the [Robots Exclusion Protocol RFC](https://datatracker.ietf.org/doc/html/draft-koster-rep) (in draft as of August 2022) which would effectively replace it.

The two RFCs have quite a lot of overlap in terms of the core rules.
Generally though, the Robots Exclusion Protocol RFC is more flexible when it comes to allowed characters (full UTF-8) and spacing.

The Robots Exclusion Tools library attempts to strike a compatibility balance for both, allowing some specific quirks of the NoRobots RFC with the expanded characterset from the Robots Exclusion Protocol RFC.

## Parsing in-request robots rules (metatags and header)
Similar to the rules from a "robots.txt" file, there can be in-request rules deciding whether a page allows indexing or following links.
The process of extracting this data from a request isn't currently part of this library, avoiding a dependency to parse HTML.

If you extract the raw rules from the metatags and `X-Robots-Tag` header, you can pass those into the parser.
The parser takes an array of rules and returns a `RobotsPageDefinition` file which allows querying of the rules by user agent.

Like the `RobotsFileParser`, this parser is built around the tokenization and validation system and is similarly extendable.

There is no RFC available to define the formats of metatag or `X-Robots-Tag` data.
The parser follows the base formatting rules described in the NoRobots and the Robots Exclusion Protocol RFCs regarding fields combined with rules from [Google's documentation on the robots metatag](https://developers.google.com/search/reference/robots_meta_tag).
There are ambiguities in the rules described there (like whether there is rule inheritence from global scope) which may be different to what other implementations may use.

## Example Usage
### Parsing a "robots.txt" file from URI
```csharp
using TurnerSoftware.RobotsExclusionTools;

var robotsFileParser = new RobotsFileParser();
RobotsFile robotsFile = await robotsFileParser.FromUriAsync(new Uri("http://www.example.org/robots.txt"));

var allowedAccess = robotsFile.IsAllowedAccess(
	new Uri("http://www.example.org/some/url/i-want-to/check"),
	"MyUserAgent"
);
```

### Parsing robots data from metatags or the `X-Robots-Tag`
```csharp
using TurnerSoftware.RobotsExclusionTools;

//These rules are gathered by you from the Robots metatag and `X-Robots-Tag` header
var pageRules = new[] {
	"noindex, notranslate",
	"googlebot: none",
	"otherbot: nofollow",
	"superbot: all"
};

var robotsPageParser = new RobotsPageParser();
RobotsPageDefinition robotsPageDefinition = robotsPageParser.FromRules(pageRules);

robotsPageDefinition.CanIndex("SomeNotListedBot/1.0"); //False
robotsPageDefinition.CanFollowLinks("SomeNotListedBot/1.0"); //True
robotsPageDefinition.HasRule("notranslate", "SomeNotListedBot/1.0"); //True

robotsPageDefinition.CanIndex("GoogleBot/1.0"); //False
robotsPageDefinition.CanFollowLinks("GoogleBot/1.0"); //False
robotsPageDefinition.HasRule("notranslate", "GoogleBot/1.0"); //True

robotsPageDefinition.CanIndex("OtherBot/1.0"); //False
robotsPageDefinition.CanFollowLinks("OtherBot/1.0"); //False
robotsPageDefinition.HasRule("notranslate", "OtherBot/1.0"); //True

robotsPageDefinition.CanIndex("superbot/1.0"); //True
robotsPageDefinition.CanFollowLinks("superbot/1.0"); //True
robotsPageDefinition.HasRule("notranslate", "superbot/1.0"); //True
```
