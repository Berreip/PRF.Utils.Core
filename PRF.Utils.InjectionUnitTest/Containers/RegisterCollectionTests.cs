using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.InjectionUnitTest.Containers
{
    [TestFixture]
    internal sealed class RegisterCollectionTests
    {
        private IInjectionContainer _sut;

        [SetUp]
        public void TestInitialize()
        {
            // software under test:
            _sut = new InjectionContainerSimpleInjector();
        }

        [Test]
        public void EmptyCollection()
        {
            //Configuration
            _sut.Register<IHandlerClass, HandlerClass>(LifeTime.Singleton);
            _sut.Register<IPluginClass, PluginClass1>(LifeTime.Singleton);

            //Test
            var res = _sut.Resolve<IHandlerClass>();

            //Verify
            Assert.AreEqual(0, res.Plugins.Count);
        }
        
        [Test]
        public void NominalCollection()
        {
            //Configuration
            _sut.Register<IHandlerClass, HandlerClass>(LifeTime.Singleton);
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Singleton, typeof(PluginClass1), typeof(PluginClass2), typeof(PluginClass3));

            //Test
            var res = _sut.Resolve<IHandlerClass>();

            //Verify
            Assert.AreEqual(3, res.Plugins.Count);
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass1));
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass2));
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass3));
        }
        
        [Test]
        public void CompletedCollection_Singleton()
        {
            //Configuration
            _sut.Register<IHandlerClass, HandlerClass>(LifeTime.Singleton);
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Singleton, typeof(PluginClass1), typeof(PluginClass2));
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Singleton, typeof(PluginClass3));

            //Test
            var res = _sut.Resolve<IHandlerClass>();

            //Verify
            Assert.AreEqual(3, res.Plugins.Count);
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass1));
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass2));
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass3));
        }
             
        [Test]
        public void CompletedCollection_Singleton_CheckInstance()
        {
            //Configuration
            _sut.Register<IHandlerClass, HandlerClass>(LifeTime.Singleton);
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Singleton, typeof(PluginClass1), typeof(PluginClass2));
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Singleton, typeof(PluginClass3));

            //Test
            var res = _sut.Resolve<IReadOnlyList<IPluginClass>>();
            var res2 = _sut.Resolve<IReadOnlyList<IPluginClass>>();

            //Verify
            for (var i = 0; i < res.Count; i++)
            {
                Assert.AreSame(res2[i], res[i]);
            }
        }
        
        [Test]
        public void CompletedCollection_Transient()
        {
            //Configuration
            _sut.Register<IHandlerClass, HandlerClass>(LifeTime.Transient);
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Transient, typeof(PluginClass1), typeof(PluginClass2));
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Transient, typeof(PluginClass3));

            //Test
            var res = _sut.Resolve<IHandlerClass>();

            //Verify
            Assert.AreEqual(3, res.Plugins.Count);
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass1));
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass2));
            Assert.IsTrue(res.Plugins.Any(o => o is PluginClass3));
        }
        
        [Test]
        public void CompletedCollection_Transient_CheckInstance()
        {
            //Configuration
            _sut.Register<IHandlerClass, HandlerClass>(LifeTime.Transient);
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Transient, typeof(PluginClass1), typeof(PluginClass2));
            _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Transient, typeof(PluginClass3));

            //Test
            var res = _sut.Resolve<IReadOnlyList<IPluginClass>>();
            var res2 = _sut.Resolve<IReadOnlyList<IPluginClass>>();

            //Verify
            for (var i = 0; i < res.Count; i++)
            {
                Assert.AreNotSame(res2[i], res[i]);
            }
        }

    }

    internal interface IHandlerClass
    {
        IReadOnlyCollection<IPluginClass> Plugins { get; }
    }

    internal sealed class HandlerClass : IHandlerClass
    {
        public IReadOnlyCollection<IPluginClass> Plugins { get; }

        public HandlerClass(IEnumerable<IPluginClass> plugins)
        {
            Plugins = plugins.ToArray();
        }
    }

    internal interface IPluginClass { }
    internal sealed class PluginClass1 : IPluginClass { }
    internal sealed class PluginClass2 : IPluginClass { }
    internal sealed class PluginClass3 : IPluginClass { }
}