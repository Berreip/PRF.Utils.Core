using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Time.Testing;
using PRF.Utils.CoreComponents.Async;

namespace PRF.Utils.CoreComponent.UnitTest.Async;

public sealed class BatchProcessingQueueTests
{
    private static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(10);
    private readonly List<int[]> _pages = new();
    private readonly CountingTimeProvider _timeProvider = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Invalid_Page_Size_Is_Rejected(int pageSize)
    {
        Assert.Throws<ArgumentException>(() => new BatchProcessingQueue<int>(pageSize, TIMEOUT, _pages.Add, _timeProvider));
    }

    [Fact]
    public void Null_TimeProvider_Is_Rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new BatchProcessingQueue<int>(10, TIMEOUT, _pages.Add, null));
    }

    [Fact]
    public void Original_Constructor_Flushes_A_Full_Page()
    {
        using var sut = new BatchProcessingQueue<int>(2, Timeout.InfiniteTimeSpan, _pages.Add);

        sut.Add(1);
        sut.Add(2);

        Assert.Equal(new[] { 1, 2 }, Assert.Single(_pages));
    }

    [Fact]
    public void Full_Pages_Preserve_All_Items_And_Disarm_The_Timer()
    {
        // Arrange
        using var sut = new BatchProcessingQueue<int>(100, TIMEOUT, _pages.Add, _timeProvider);

        // Act
        for (var i = 0; i < 7000; i++)
        {
            sut.Add(i);
        }
        _timeProvider.Advance(TimeSpan.FromHours(1));

        // Assert
        Assert.Equal(70, _pages.Count);
        Assert.All(_pages, page => Assert.Equal(100, page.Length));
        Assert.Equal(Enumerable.Range(0, 7000), _pages.SelectMany(page => page));
        Assert.Equal(0, _timeProvider.CallbackCount);
    }

    [Fact]
    public void Empty_Queue_Does_Not_Wake_And_First_Item_Starts_The_Timeout()
    {
        // Arrange
        using var sut = new BatchProcessingQueue<int>(10, TIMEOUT, _pages.Add, _timeProvider);
        _timeProvider.Advance(TimeSpan.FromHours(1));
        Assert.Equal(0, _timeProvider.CallbackCount);

        // Act / Assert
        sut.Add(1);
        _timeProvider.Advance(TimeSpan.FromSeconds(9));
        Assert.Empty(_pages);
        Assert.Equal(0, _timeProvider.CallbackCount);

        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(new[] { 1 }, Assert.Single(_pages));
        Assert.Equal(1, _timeProvider.CallbackCount);

        _timeProvider.Advance(TimeSpan.FromHours(1));
        Assert.Single(_pages);
        Assert.Equal(1, _timeProvider.CallbackCount);
    }

    [Fact]
    public void Regular_Additions_Do_Not_Postpone_The_First_Item_Deadline()
    {
        // Arrange
        using var sut = new BatchProcessingQueue<int>(10, TIMEOUT, _pages.Add, _timeProvider);
        sut.Add(1);

        // Act
        for (var i = 2; i <= 4; i++)
        {
            _timeProvider.Advance(TimeSpan.FromSeconds(3));
            sut.Add(i);
        }
        Assert.Empty(_pages);
        _timeProvider.Advance(TimeSpan.FromSeconds(1));

        // Assert
        Assert.Equal(new[] { 1, 2, 3, 4 }, Assert.Single(_pages));
        Assert.Equal(1, _timeProvider.CallbackCount);
    }

    [Fact]
    public void ForceFlush_Emits_Only_Pending_Items_And_Disarms_The_Timer()
    {
        // Arrange
        using var sut = new BatchProcessingQueue<int>(10, TIMEOUT, _pages.Add, _timeProvider);
        sut.ForceFlush();
        Assert.Empty(_pages);
        sut.Add(1);
        sut.Add(2);

        // Act
        sut.ForceFlush();
        sut.ForceFlush();
        _timeProvider.Advance(TimeSpan.FromHours(1));

        // Assert
        Assert.Equal(new[] { 1, 2 }, Assert.Single(_pages));
        Assert.Equal(0, _timeProvider.CallbackCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void New_Page_Starts_A_Fresh_Timeout_After_Flushing(bool fillPage)
    {
        // Arrange
        using var sut = new BatchProcessingQueue<int>(2, TIMEOUT, _pages.Add, _timeProvider);
        sut.Add(1);
        _timeProvider.Advance(TimeSpan.FromSeconds(6));
        if (fillPage)
        {
            sut.Add(2);
        }
        else
        {
            sut.ForceFlush();
        }
        Assert.Single(_pages);

        // Act / Assert
        sut.Add(3);
        _timeProvider.Advance(TimeSpan.FromSeconds(4));
        // L'ancienne échéance ne doit ni réveiller le timer ni vider la nouvelle page.
        Assert.Single(_pages);
        Assert.Equal(0, _timeProvider.CallbackCount);

        _timeProvider.Advance(TimeSpan.FromSeconds(6));
        Assert.Equal(2, _pages.Count);
        Assert.Equal(new[] { 3 }, _pages[1]);
        Assert.Equal(1, _timeProvider.CallbackCount);
    }

    [Fact]
    public void New_Page_Can_Time_Out_After_A_Previous_Timeout()
    {
        using var sut = new BatchProcessingQueue<int>(10, TIMEOUT, _pages.Add, _timeProvider);
        sut.Add(1);
        _timeProvider.Advance(TIMEOUT);

        sut.Add(2);
        _timeProvider.Advance(TIMEOUT);

        Assert.Equal(2, _pages.Count);
        Assert.Equal(new[] { 1 }, _pages[0]);
        Assert.Equal(new[] { 2 }, _pages[1]);
        Assert.Equal(2, _timeProvider.CallbackCount);
    }

    [Fact]
    public void Infinite_Timeout_Requires_An_Explicit_Or_Full_Page_Flush()
    {
        using var sut = new BatchProcessingQueue<int>(10, Timeout.InfiniteTimeSpan, _pages.Add, _timeProvider);
        sut.Add(1);

        _timeProvider.Advance(TimeSpan.FromDays(1));
        Assert.Empty(_pages);
        Assert.Equal(0, _timeProvider.CallbackCount);
        sut.ForceFlush();

        Assert.Equal(new[] { 1 }, Assert.Single(_pages));
    }

    [Fact]
    public void Dispose_Abandons_Pending_Items_And_Stops_The_Timer()
    {
        var sut = new BatchProcessingQueue<int>(10, TIMEOUT, _pages.Add, _timeProvider);
        sut.Add(1);

        sut.Dispose();
        sut.Dispose();
        sut.ForceFlush();
        _timeProvider.Advance(TimeSpan.FromHours(1));

        Assert.Empty(_pages);
        Assert.Equal(0, _timeProvider.CallbackCount);
        Assert.Throws<ObjectDisposedException>(() => sut.Add(2));
    }

    // Compter les callbacks du timer permet de détecter les réveils à vide,
    // même lorsqu'ils ne produisent aucune page visible par le consommateur.
    private sealed class CountingTimeProvider : FakeTimeProvider
    {
        public int CallbackCount { get; private set; }

        public override ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return base.CreateTimer(InvokeCallback, new CallbackState(this, callback, state), dueTime, period);
        }

        private static void InvokeCallback(object state)
        {
            var invocation = (CallbackState)state;
            invocation.Provider.CallbackCount++;
            invocation.Callback(invocation.State);
        }

        private sealed record CallbackState(CountingTimeProvider Provider, TimerCallback Callback, object State);
    }
}
