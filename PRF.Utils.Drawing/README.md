# PRF.Utils.Drawing

Bitmap extensions moved out of CoreComponents 3.0.0 and kept here for future use.
The namespace is now `PRF.Utils.Drawing.Extensions`.

This project targets .NET 10 and uses `System.Drawing.Common`, which requires
Windows for these operations. CoreComponents does not reference this project.

`IsPackable` and `GeneratePackageOnBuild` are disabled. No NuGet package is produced
or published for this project. Its existing tests live in `PRF.Utils.Drawing.UnitTest`.
