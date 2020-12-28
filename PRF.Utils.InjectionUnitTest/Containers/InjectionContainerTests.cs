using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;

namespace PRF.Utils.InjectionUnitTest.Containers
{
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
            var sut = new InjectionContainer();

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
            var sut = new InjectionContainer();

            // Act 
            sut.RegisterType<FooType>(LifeTime.Singleton);
            for (int i = 0; i < 10; i++)
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
            var sut = new InjectionContainer();

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
            var sut = new InjectionContainer();

            // Act 
            sut.RegisterType<FooType>(LifeTime.Transient);
            for (int i = 0; i < 10; i++)
            {
                var foo = sut.Resolve<FooType>();
                Assert.IsNotNull(foo);
            }

            // Assert
            Assert.AreEqual(10, _counter);
        }

        private class FooType
        {
            public FooType()
            {
                _counter++;
            }
        }
    }
}
