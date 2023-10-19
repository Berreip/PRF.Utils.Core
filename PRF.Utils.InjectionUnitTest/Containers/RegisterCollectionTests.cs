using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.InjectionUnitTest.Containers;

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
        
    [Test]
    public void RegisterOrAppendCollection_CheckInstance_when_mixed_singleton_transient()
    {
        //Configuration
        _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Singleton, typeof(PluginClass1));
        _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Transient, typeof(PluginClass2));

        //Test
        var res = _sut.Resolve<IReadOnlyList<IPluginClass>>();
        var res2 = _sut.Resolve<IReadOnlyList<IPluginClass>>();

        //Verify
        for (var i = 0; i < res.Count; i++)
        {
            var expected = res2[i];
            if (expected is PluginClass1)
            {
                Assert.AreSame(expected, res[i]);
            }
            else
            {
                Assert.AreNotSame(expected, res[i]);
            }
        }
    }
    
    [Test]
    public void RegisterOrAppendCollection_works_when_registering_instances()
    {
        //Configuration
        var pluginClass1 = new PluginClass1();
        var pluginClass2 = new PluginClass2();
        var pluginClass3 = new PluginClass3();
            
        _sut.RegisterOrAppendCollectionInstances<IPluginClass>(pluginClass1, pluginClass2, pluginClass3);

        //Test
        var res = _sut.Resolve<IReadOnlyList<IPluginClass>>();

        //Verify
        Assert.AreEqual(3, res.Count);
        Assert.AreSame(pluginClass1, res[0]);
        Assert.AreSame(pluginClass2, res[1]);
        Assert.AreSame(pluginClass3, res[2]);
    } 
        
    [Test]
    public void RegisterOrAppendCollection_does_not_throw_for_empty_list()
    {
        //Configuration

        //Test
        _sut.RegisterOrAppendCollectionInstances<IPluginClass>();

        //Verify
    }
        
        
    [Test]
    public void RegisterOrAppendCollection_works_when_mixing_instance_registrations_and_type()
    {
        //Configuration
        var pluginClass1 = new PluginClass1();
        var pluginClass2 = new PluginClass2();
            
        _sut.RegisterOrAppendCollectionInstances<IPluginClass>(pluginClass1, pluginClass2);
        _sut.RegisterOrAppendCollection<IPluginClass>(LifeTime.Singleton, typeof(PluginClass3));

        //Test
        var res = _sut.Resolve<IReadOnlyList<IPluginClass>>();

        //Verify
        Assert.AreEqual(3, res.Count);
        Assert.AreSame(pluginClass1, res[0]);
        Assert.AreSame(pluginClass2, res[1]);
        Assert.IsTrue(res[2] is PluginClass3);
    } 
}

internal interface IHandlerClass
{
    IReadOnlyCollection<IPluginClass> Plugins { get; }
}

// ReSharper disable once ClassNeverInstantiated.Global
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