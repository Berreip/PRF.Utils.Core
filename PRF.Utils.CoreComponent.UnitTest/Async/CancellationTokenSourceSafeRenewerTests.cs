using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Async;

namespace PRF.Utils.CoreComponent.UnitTest.Async;

internal sealed class CancellationTokenSourceRenewerTests
{
    private CancellationTokenSourceSafeRenewer _sut;

    [SetUp]
    public void TestInitialize()
    {
        _sut = new CancellationTokenSourceSafeRenewer();
    }


    [Test]
    public void ctor_nominalUsage()
    {
        //Arrange

        //Act
        var res = _sut.GetNewTokenAndCancelPrevious();

        //Assert
        Assert.IsNotNull(res);
    }

    [Test]
    public void ctor_nominalUsage_call_twice_do_not_returns_same_token()
    {
        //Arrange

        //Act
        var token1 = _sut.GetNewTokenAndCancelPrevious();
        var token2 = _sut.GetNewTokenAndCancelPrevious();

        //Assert
        Assert.IsTrue(token1.IsCancellationRequested);
        Assert.IsFalse(token2.IsCancellationRequested);
    }

    [Test]
    public void GetNewTokenAndCancelPrevious_ShouldCreateNewTokenAndCancelPrevious()
    {
        // Arrange
        var previousToken = _sut.GetToken();

        // Act
        var newToken = _sut.GetNewTokenAndCancelPrevious();

        // Assert
        Assert.AreNotSame(previousToken, newToken);
        Assert.IsTrue(previousToken.IsCancellationRequested);
        Assert.IsFalse(newToken.IsCancellationRequested);
    }

    [Test]
    public void GetToken_ShouldReturnLinkedToken()
    {
        // Arrange
        var previousToken = _sut.GetToken();

        // Act
        var token = _sut.GetToken();

        // Assert
        Assert.IsFalse(previousToken.IsCancellationRequested);
        Assert.IsFalse(token.IsCancellationRequested);
    }

    [Test]
    public void GetToken_ShouldReturnLinkedToken_both_cancelled_after_Cancel()
    {
        // Arrange
        var previousToken = _sut.GetToken();
        var token = _sut.GetToken();

        // Act
        _sut.Cancel();

        // Assert
        Assert.IsTrue(previousToken.IsCancellationRequested);
        Assert.IsTrue(token.IsCancellationRequested);
    }

    [Test]
    public void GetToken_WhenNoPreviousToken_ShouldCreateNewToken()
    {
        // Act
        var token = _sut.GetToken();

        // Assert
        Assert.IsNotNull(token);
    }

    [Test]
    public void Cancel_ShouldCancelPreviousToken()
    {
        // Arrange
        var previousToken = _sut.GetToken();

        // Act
        _sut.Cancel();

        // Assert
        Assert.IsTrue(previousToken.IsCancellationRequested);
    }

    [Test]
    public void Cancel_WhenNoPreviousToken_ShouldNotThrowException()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _sut.Cancel());
    }

    [Test]
    public void GetToken_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var token = _sut.GetToken();
        _sut.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.Delay(100, token).ConfigureAwait(false));
    }
}