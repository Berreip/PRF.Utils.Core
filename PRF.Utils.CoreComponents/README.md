# CoreComponents details

## Version 3.0.0

CoreComponents now targets **.NET 10**. Consumers must target .NET 10 or later;
applications using .NET Framework or .NET 8/9 must remain on the 2.x package.
The other PRF packages keep their existing target frameworks.

The bitmap extensions use `System.Drawing.Common` and are supported on Windows only.

This module is available as a Nuget package: [PRF.Utils.CoreComponents](https://www.nuget.org/packages/PRF.Utils.CoreComponents)

Its mains purpose is to provide extensions methods around DirectoryInfo, FileInfo, some basics types and JSON and XML manipulation and some helpers for async dispatch

## Batch processing and TimeProvider

`BatchProcessingQueue<T>` accepts a `TimeProvider` as its fourth constructor argument.
The existing three-argument constructor uses `TimeProvider.System`.

The timer stays inactive while the queue is empty. The first item starts a one-shot
timeout; subsequent items do not postpone it. Reaching the page size or calling
`ForceFlush()` flushes the page and cancels its timeout. The next page starts a new
timeout with its first item. `Timeout.InfiniteTimeSpan` disables timeout flushing.

The page callback runs synchronously outside the queue lock. Dispatch work from it
when asynchronous processing is needed. Callbacks can overlap when producers or
timer callbacks run concurrently; consumers needing serial processing must provide it.
Disposing the queue cancels the timer without flushing pending items. Call
`ForceFlush()` before disposal when those items must be processed.

Tests can supply `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`
and advance time explicitly, without real delays.

