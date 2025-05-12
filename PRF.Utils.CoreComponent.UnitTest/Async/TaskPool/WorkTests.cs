using PRF.Utils.CoreComponents.Async.TaskPool;

namespace PRF.Utils.CoreComponent.UnitTest.Async.TaskPool;

public class WorkTestsCancellationAfterDispose
{
    [Fact]
    public void WorkDisposeTest()
    {
        //Arrange
        using (var work = new Work(_ => { }))
        {
            //Act
            work.Cancel();
        }

        //Assert
    }

    [Fact]
    public void WorkDisposeTest_then_request_cancel()
    {
        //Arrange
        var work = new Work(_ => { });
        work.Dispose();

        //Act
        work.Cancel();

        //Assert
    }

    [Fact]
    public void WorkDisposeTest_then_request_IsCancellationRequested()
    {
        //Arrange
        var work = new Work(_ => { });
        work.Dispose();

        //Act
        var state = work.IsCancellationRequested;

        //Assert
        Assert.False(state);
    }

    [Fact]
    public void WorkDisposeTest_then_request_IsCancellationRequested_after_cancellation()
    {
        //Arrange
        var work = new Work(_ => { });
        work.Cancel();
        work.Dispose();

        //Act
        var state = work.IsCancellationRequested;

        //Assert
        Assert.True(state);
    }
}