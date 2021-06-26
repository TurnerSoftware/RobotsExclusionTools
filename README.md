# Robots Exclusion Tools
A "robots.txt" parsing and querying library in C#, closely following the [NoRobots RFC](http://www.robotstxt.org/norobots-rfc.txt) and other details on [robotstxt.org](http://www.robotstxt.org/robotstxt.html).

[![AppVeyor](https://img.shields.io/appveyor/ci/Turnerj/robotsexclusiontools/main.svg)](https://ci.appveyor.com/project/Turnerj/robotsexclusiontools)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/robotsexclusiontools/main.svg)](https://codecov.io/gh/TurnerSoftware/RobotsExclusionTools)
[![NuGet](https://img.shields.io/nuget/v/TurnerSoftware.RobotsExclusionTools.svg)](https://www.nuget.org/packages/TurnerSoftware.RobotsExclusionTools)

## Features
- Load Robots by string, by URI (Async) or by streams (Async)
- Supports multiple user-agents and "*"
- Supports `Allow` and `Disallow`
- Supports `Crawl-delay` entries
- Supports `Sitemap` entries
- Supports wildcard paths (*) as well as must-end-with declarations ($)
- Built-in "robots.txt" tokenization system (allowing extension to support other custom fields)
- Built-in "robots.txt" validator (allowing to validate a tokenized file)
- Dedicated parser for the data from `<meta name="robots" />` tag and the `X-Robots-Tag` header

## NoRobots RFC Compatibility
This library attempts to stick closely to the rules defined in the RFC document, including:
- Global/any user-agent when none is explicitly defined (Section 3.2.1 of RFC)
- Field names (eg. "User-agent") are character restricted (Section 3.3)
- Allow/disallow rules are performed by order-of-occurence (Section 3.2.2)
- Loading by URI applies default rules based on access to "robots.txt" (Section 3.1)
- Interoperability for varying line endings (Section 5.2)

## Tokenization & Validation
At the core of the library is a tokenization system to parse the file format. 
It follows the formal syntax rules defined in Section 3.3 of the NoRobots RFC to the characters that are valid.
When used in conjunction with the token validator, it can enforce the correct token structure too.

The major benefit for designing the library around this system is that is allows for greater extendability.
If you wanted to support custom fields that the core `RobotsFile` class didn't use, you can parse the data with the tokenizer.

## Parsing in-request robots rules (metatags and header)
Similar to the rules from a "robots.txt" file, there can be in-request rules deciding whether a page allows indexing or following links.
The process of extracting this data from a request isn't currently part of this library, avoiding a dependency to parse HTML.

If you extract the raw rules from the metatags and `X-Robots-Tag` header, you can pass those into the parser.
The parser takes an array of rules and returns a `RobotsPageDefinition` file which allows querying of the rules by user agent.

Like the `RobotsFileParser`, this parser is built around the tokenization and validation system and is similarly extendable.

There is no RFC available to define the formats of metatag or `X-Robots-Tag` data.
The parser follows the base formatting rules described in the NoRobots RFC regarding fields combined with rules from [Google's documentation on the robots metatag](https://developers.google.com/search/reference/robots_meta_tag).
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
robotsPageDefinition.Can("translate", "SomeNotListedBot/1.0"); //False

robotsPageDefinition.CanIndex("GoogleBot/1.0"); //False
robotsPageDefinition.CanFollowLinks("GoogleBot/1.0"); //False
robotsPageDefinition.Can("translate", "GoogleBot/1.0"); //False

robotsPageDefinition.CanIndex("OtherBot/1.0"); //False
robotsPageDefinition.CanFollowLinks("OtherBot/1.0"); //False
robotsPageDefinition.Can("translate", "OtherBot/1.0"); //False

robotsPageDefinition.CanIndex("superbot/1.0"); //True
robotsPageDefinition.CanFollowLinks("superbot/1.0"); //True
robotsPageDefinition.Can("translate", "superbot/1.0"); //True
```
