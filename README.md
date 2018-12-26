# Robots Exclusion Tools
A "robots.txt" parsing and querying library in C#. This implementation closely follows the [NoRobots RFC](http://www.robotstxt.org/norobots-rfc.txt) and other information on [robotstxt.org](http://www.robotstxt.org/robotstxt.html).

[![AppVeyor](https://img.shields.io/appveyor/ci/Turnerj/robotsexclusiontools/master.svg)](https://ci.appveyor.com/project/Turnerj/robotsexclusiontools)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/robotsexclusiontools/master.svg)](https://codecov.io/gh/TurnerSoftware/RobotsExclusionTools)

## Features
- Load Robots by string, by URI (following Section 3.1 of NoRobots RFC around access rules) or by streams
- Supports multiple user-agents and "*" (Section 3.2.1 of NoRobots RFC)
- Supports user-agent fallback
- Supports `Allow` and `Disallow`, processed by order-of-occurence (Section 3.2.2 of NoRobots RFC)
- Supports `Crawl-delay` entries
- Supports `Sitemap` entries
- Supports wildcard paths (*) as well as must-end-with declarations ($)
- Built-in "robots.txt" tokenization system (allowing extension to support other custom fields)
- Built-in "robots.txt" validator (allowing to validate a tokenized file)

## Tokenization System & Validator
At the core of the library is a tokenization system to parse the file format.
This engine follows formal syntax rules defined in Section 3.3 of the NoRobots RFC to the characters that are valid per section.

This system allows for extension from the base library if you wanted to support additional custom fields that aren't specifically catered for.

Additionally, the validator provides support for specific token patterns to help re-enforce the RFC rules.

## Example Usage
```csharp

RobotsFile robotsFile = await new RobotsParser().FromUriAsync(new Uri("http://www.example.org/robots.txt"));
var allowedAccess = robotsFile.IsAllowedAccess(new Uri("http://www.example.org/some/url/i-want-to/check"), "MyUserAgent");

```