using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PRF.Utils.CoreComponents.Async;

namespace PRF.Utils.CoreComponent.UnitTest.Async;

public sealed class BatchProcessingQueueTests
{
    private readonly ConcurrentQueue<int?[]> _pageList;

    public BatchProcessingQueueTests()
    {
        _pageList = new ConcurrentQueue<int?[]>();
    }

    [Fact]
    public void Add_Item_Flushes_When_PageSize_Reached()
    {
        // Arrange
        var mrev = new ManualResetEventSlim();
        using var sut = new BatchProcessingQueue<int?>(
            pageMaximumSize: 100,
            TimeSpan.FromMinutes(1),
            page =>
            {
                _pageList.Enqueue(page);
                mrev.Set();
            });

        // Act
        for (var i = 0; i < 100; i++)
        {
            sut.Add(i);
        }

        var pageRetrieved = mrev.Wait(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(pageRetrieved);
        Assert.True(_pageList.TryDequeue(out var singlePage));
        Assert.False(_pageList.TryDequeue(out _));
        Assert.Equal(100, singlePage.Length);
        Assert.Equal(100, singlePage.Count(o => o != null));
    }

    [Fact]
    public void Add_Item_Flushes_When_Multiple_PagesSize_Reached()
    {
        // Arrange
        var mrev = new ManualResetEventSlim();
        const int pageMaximumSize = 100;
        const int nbPageToGenerate = 70;
        using var sut = new BatchProcessingQueue<int?>(
            pageMaximumSize: pageMaximumSize,
            TimeSpan.FromMinutes(1),
            page =>
            {
                _pageList.Enqueue(page);
                if (_pageList.Count == nbPageToGenerate)
                {
                    mrev.Set();
                }
            });

        // Act
        const int nbItems = pageMaximumSize*nbPageToGenerate;

        for (var i = 0; i < nbItems; i++)
        {
            sut.Add(i);
        }

        var pageRetrieved = mrev.Wait(TimeSpan.FromSeconds(15));

        // Assert
        Assert.True(pageRetrieved);
        // should be nbPageToGenerate queued
        for (var i = 0; i < nbPageToGenerate; i++)
        {
            Assert.True(_pageList.TryDequeue(out var singlePage));
            Assert.Equal(100, singlePage.Length);
            Assert.Equal(100, singlePage.Count(o => o != null));
        }
        // and no more page after
        Assert.False(_pageList.TryDequeue(out _));
    }

    [Fact]
    public void Add_Item_Does_Not_Flush_Before_PageSize_Reached()
    {
        // Arrange
        var mrev = new ManualResetEventSlim();
        using var sut = new BatchProcessingQueue<int?>(
            pageMaximumSize: 100,
            TimeSpan.FromMinutes(10),
            page =>
            {
                _pageList.Enqueue(page);
                mrev.Set();
            });

        // Act
        sut.Add(1);
        sut.Add(2);
        var pageRetrieved = mrev.Wait(TimeSpan.FromMilliseconds(400));

        // Assert
        Assert.False(pageRetrieved);
        Assert.False(_pageList.TryDequeue(out _));
    }

    [Fact]
    public void Add_Item_Does_Flush_Before_PageSize_When_timeout_is_Reached()
    {
        // Arrange
        var mrev = new ManualResetEventSlim();
        using var sut = new BatchProcessingQueue<int?>(
            pageMaximumSize: 100,
            TimeSpan.FromMilliseconds(100),
            page =>
            {
                _pageList.Enqueue(page);
                mrev.Set();
            });

        // Act
        sut.Add(1);
        var pageRetrieved = mrev.Wait(TimeSpan.FromMilliseconds(400));

        // Assert
        Assert.True(pageRetrieved);
        Assert.True(_pageList.TryDequeue(out var singlePage));
        Assert.Single(singlePage);
        Assert.Equal(1, singlePage.Count(o => o != null));
        // and no more page after
        Assert.False(_pageList.TryDequeue(out _));
    }

    [Fact]
    public void ForceFlush_empty_the_queue()
    {
        // Arrange
        var mrev = new ManualResetEventSlim();
        using var sut = new BatchProcessingQueue<int?>(
            pageMaximumSize: 100,
            Timeout.InfiniteTimeSpan,
            page =>
            {
                _pageList.Enqueue(page);
                mrev.Set();
            });

        sut.Add(1);

        // Act
        sut.ForceFlush();
        var pageRetrieved = mrev.Wait(TimeSpan.FromMilliseconds(400));

        // Assert
        Assert.True(pageRetrieved);
        Assert.True(_pageList.TryDequeue(out var singlePage));
        Assert.Single(singlePage);
        Assert.Equal(1, singlePage.Count(o => o != null));
        // and no more page after
        Assert.False(_pageList.TryDequeue(out _));
    }

    [Fact]
    public async Task Ensure_that_regular_adding_of_items_do_not_prevent_timer_from_beeing_raised()
    {
        // Arrange
        var mrev = new ManualResetEventSlim();
        using var sut = new BatchProcessingQueue<int?>(
            pageMaximumSize: 100_000,
            TimeSpan.FromMilliseconds(300),
            page =>
            {
                _pageList.Enqueue(page);
                mrev.Set();
            });
        var cts =new CancellationTokenSource();

        // Act
        sut.Add(1);
        // ReSharper disable once MethodSupportsCancellation
        var task = Task.Run(async () =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    sut.Add(1);
                    await Task.Delay(20, cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // nominal cancellation
            }
        });

        var pageRetrieved = mrev.Wait(TimeSpan.FromSeconds(1));
        await cts.CancelAsync().ConfigureAwait(true);
        await task.ConfigureAwait(true);

        // Assert
        Assert.True(pageRetrieved);
        Assert.True(_pageList.TryDequeue(out _));
        // no check on content as is may vary widely depending on the delay: we ensure that at leat one raised has been done
    }
}