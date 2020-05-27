# Faithlife.NestEmbedder

Automatically embed Nest and Elasticsearch.Net assemblies, so you can upgrade your Elasticsearch servers more easily.

Documentation: https://faithlife.github.io/FaithlifeNestEmbedder/

Build | NuGet
--- | ---
[![Build](https://github.com/Faithlife/NestEmbedder/workflows/Build/badge.svg)](https://github.com/Faithlife/NestEmbedder/actions?query=workflow%3ABuild) | [![NuGet](https://img.shields.io/nuget/v/Faithlife.NestEmbedder.svg)](https://www.nuget.org/packages/Faithlife.NestEmbedder)

# How to use

When using Elasticsearch, the `Nest` (and `Elasticsearch.Net`) packages must be the same major version as your Elasticsearch server. This causes a problem when bringing an updated Elasticsearch server online: the existing code can only talk to either the old server or the new server, not both. This package helps solve that issue by embedding the `Nest` and `Elasticsearch.Net` packages, allowing multiple versions to be used by the same application.

The first step is to isolate your old Nest calls into a separate dll. E.g., if your search logic is in the `SearchLogic` project, and you're upgrading from Elasticsearch v5 to v6, then you would first factor out all Nest usage into a new project `SearchLogic.Nest5`, and have `SearchLogic` depend on `SearchLogic.Nest5`. Remove all references to Nest from `SearchLogic` and ensure that `SearchLogic.Nest5` does not expose any Nest types in its API.

Next, install the [`Faithlife.NestEmbedder`](https://www.nuget.org/packages/Faithlife.NestEmbedder) package into `SearchLogic.Nest5`. This will embed and isolate the v5 assemblies into that project. For each externally-visible type in `SearchLogic.Nest5`, add a static constructor that calls `Faithlife.NestEmbedder.EmbeddedAssemblyLoader.AssemblyLoader.LoadAll()`. At this point, build and test to ensure the solution is working.

Now you're ready to add the v6 Nest assemblies. You can choose to add them to `SearchLogic` directly, or you can create a new `SearchLogic.Nest6` project. If you do the `SearchLogic.Nest6` project, you can also install `Faithlife.NestEmbedder` into that project to isolate the v6 assemblies as well.

The end result: you have a `SearchLogic` project that can search on either Elasticsearch v5 (via `SearchLogic.Nest5`) or Elasticsearch v6 (via a direct v6 Nest reference, or via `SearchLogic.Nest6`).

# How it works

There are three parts to isolating Nest assemblies:

1. Auto-generated binding redirects for Nest and Elasticsearch are always wrong, since they assume code always wants to use the latest version. `Faithlife.NestEmbedder` removes all auto-generated binding redirects for Nest and Elasticsearch both for the project it is installed into and all dependent projects.
1. For only the project `Faithlife.NestEmbedder` is installed into, `Faithlife.NestEmbedder` will [make the Nest and Elasticsearch package assets private](https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#controlling-dependency-assets). This ensures that dependent projects do not inherit those package references.
1. For only the project `Faithlife.NestEmbedder` is installed into, when the project is built, the `Nest.dll` and `Elasticsearch.Net.dll` assemblies are packaged as resources into the project's assembly.

Then at runtime, when that project calls `Faithlife.NestEmbedder.EmbeddedAssemblyLoader.LoadAll()`, its embedded assemblies are retrieved and loaded into the application domain.

# More info

* License: [MIT](LICENSE)
* [Version History](VersionHistory.md)
* [Contributing Guidelines](CONTRIBUTING.md)
