# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Security
### Added
- Initial project setup - Roslyn-based tool to find unused public types from NuGet packages in a .NET solution
- Unit tests for AnalysisSummary, TypeResult, TypeKind, AnalysisResult, and JsonOutput
- Walk compiled NuGet dependency assembly metadata using PEReader to find transitive type references that are not visible in source code
### Fixed
### Changed
- SDK - Updated DotNet SDK to 10.0.302
### Deprecated
### Removed
### Deployment Changes
<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created