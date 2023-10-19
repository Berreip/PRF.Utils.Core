using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.InjectionUnitTest.Containers;

[TestFixture]
internal sealed class InjectionContainerTests
{
    private static int _counter;

    [SetUp]
    public void TestInitialize()
    {
        _counter = 0;
    }

    [Test]
    public void InjectionContainer_Register_Nominal()
    {
        // Arrange
        var sut = new InjectionContainerSimpleInjector();

        // Act 
        sut.RegisterType<FooType>(LifeTime.Singleton);
        var foo = sut.Resolve<FooType>();

        // Assert
        Assert.IsNotNull(foo);
        Assert.AreEqual(1, _counter);
    }

    [Test]
    public void InjectionContainer_Register_Resolve_Multiple_Time()
    {
        // Arrange
        var sut = new InjectionContainerSimpleInjector();

        // Act 
        sut.RegisterType<FooType>(LifeTime.Singleton);
        for (var i = 0; i < 10; i++)
        {
            var foo = sut.Resolve<FooType>();
            Assert.IsNotNull(foo);
        }

        // Assert
        Assert.AreEqual(1, _counter);
    }

    [Test]
    public void InjectionContainer_Register_Resolve_Multiple_Time_Transient_Repro_Bug_Double_Resolve()
    {
        // Arrange
        var sut = new InjectionContainerSimpleInjector();

        // Act 
        sut.RegisterType<FooType>(LifeTime.Transient);
        var foo = sut.Resolve<FooType>();

        // Assert
        Assert.IsNotNull(foo);
        Assert.AreEqual(1, _counter);
    }

    [Test]
    public void InjectionContainer_Register_Resolve_Multiple_Time_Transient()
    {
        // Arrange
        var sut = new InjectionContainerSimpleInjector();

        // Act 
        sut.RegisterType<FooType>(LifeTime.Transient);
        for (var i = 0; i < 10; i++)
        {
            var foo = sut.Resolve<FooType>();
            Assert.IsNotNull(foo);
        }

        // Assert
        Assert.AreEqual(10, _counter);
    }
        
    [Test]
    public void InjectionContainer_Register_Collection()
    {
        // Arrange
        var sut = new InjectionContainerSimpleInjector();

        // Act 
        sut.RegisterType<FooType>(LifeTime.Transient);
        for (var i = 0; i < 10; i++)
        {
            var foo = sut.Resolve<FooType>();
            Assert.IsNotNull(foo);
        }

        // Assert
        Assert.AreEqual(10, _counter);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class FooType
    {
        public FooType()
        {
            _counter++;
        }
    }
}