using System.Threading.Tasks;
using PRF.Utils.CoreComponents.Async;

namespace PRF.Utils.CoreComponent.UnitTest.Async;

#pragma warning disable xUnit2002
#pragma warning disable xUnit2005
public sealed class CancellationTokenSourceRenewerTests
{
    private readonly CancellationTokenSourceSafeRenewer _sut = new CancellationTokenSourceSafeRenewer();


    [Fact]
    public void ctor_nominalUsage()
    {
        //Arrange

        //Act
        var res = _sut.GetNewTokenAndCancelPrevious();

        //Assert
        Assert.NotNull(res);
    }

    [Fact]
    public void ctor_nominalUsage_call_twice_do_not_returns_same_token()
    {
        //Arrange

        //Act
        var token1 = _sut.GetNewTokenAndCancelPrevious();
        var token2 = _sut.GetNewTokenAndCancelPrevious();

        //Assert
        Assert.True(token1.IsCancellationRequested);
        Assert.False(token2.IsCancellationRequested);
    }

    [Fact]
    public void GetNewTokenAndCancelPrevious_ShouldCreateNewTokenAndCancelPrevious()
    {
        // Arrange
        var previousToken = _sut.GetToken();

        // Act
        var newToken = _sut.GetNewTokenAndCancelPrevious();

        // Assert
        Assert.NotSame(previousToken, newToken);
        Assert.True(previousToken.IsCancellationRequested);
        Assert.False(newToken.IsCancellationRequested);
    }

    [Fact]
    public void GetToken_ShouldReturnLinkedToken()
    {
        // Arrange
        var previousToken = _sut.GetToken();

        // Act
        var token = _sut.GetToken();

        // Assert
        Assert.False(previousToken.IsCancellationRequested);
        Assert.False(token.IsCancellationRequested);
    }

    [Fact]
    public void GetToken_ShouldReturnLinkedToken_both_cancelled_after_Cancel()
    {
        // Arrange
        var previousToken = _sut.GetToken();
        var token = _sut.GetToken();

        // Act
        _sut.Cancel();

        // Assert
        Assert.True(previousToken.IsCancellationRequested);
        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public void GetToken_WhenNoPreviousToken_ShouldCreateNewToken()
    {
        // Act
        var token = _sut.GetToken();

        // Assert
        Assert.NotNull(token);
    }

    [Fact]
    public void Cancel_ShouldCancelPreviousToken()
    {
        // Arrange
        var previousToken = _sut.GetToken();

        // Act
        _sut.Cancel();

        // Assert
        Assert.True(previousToken.IsCancellationRequested);
    }

    [Fact]
    public void Cancel_WhenNoPreviousToken_ShouldNotThrowException()
    {
        // Act & Assert should not throw
        _sut.Cancel();
    }

    [Fact]
    public async Task GetToken_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var token = _sut.GetToken();
        _sut.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.Delay(100, token).ConfigureAwait(false)).ConfigureAwait(true);
    }
}
#pragma warning restore xUnit2002
#pragma warning restore xUnit2005