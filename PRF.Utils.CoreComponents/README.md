# CoreComponents details

## Version 3.0.0

CoreComponents now targets **.NET 10**. Consumers must target .NET 10 or later;
applications using .NET Framework or .NET 8/9 must remain on the 2.x package.
The other PRF packages keep their existing target frameworks.

The bitmap extensions use `System.Drawing.Common` and are supported on Windows only.

This module is available as a Nuget package: [PRF.Utils.CoreComponents](https://www.nuget.org/packages/PRF.Utils.CoreComponents)

Its mains purpose is to provide extensions methods around DirectoryInfo, FileInfo, some basics types and JSON and XML manipulation and some helpers for async dispatch

