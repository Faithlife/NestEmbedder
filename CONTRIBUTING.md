# Contributing to FaithlifeNestEmbedder

## Prerequisites

* Install [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/) with the [editorconfig extension](https://github.com/editorconfig/editorconfig-vscode).
* Install [.NET Core 3.1.x](https://dotnet.microsoft.com/download).

## Guidelines

* All public classes, methods, interfaces, enums, etc. **must** have correct XML documentation comments.
* Update [VersionHistory](VersionHistory.md) with a human-readable description of the change.

## How to Build

* `git clone https://github.com/Faithlife/FaithlifeNestEmbedder.git`
* `cd FaithlifeNestEmbedder`
* `dotnet pack`

## Creating a Release

First, ensure [VersionHistory](VersionHistory.md) is udpated. Then:

* `git tag -a -m "v1.0.0" v1.0.0`
* `git push --tags`
