using System;
using NUnit.Framework;

namespace PRF.Utils.CoreComponent.UnitTest.UriUtils
{
    [TestFixture]
    public sealed class UriCheckTests
    {
        [Test]
        [TestCase(@"C:\toto\file.txt")]
        [TestCase(@"chrome://branding/content/about-logo.png")]
        [TestCase(@"https://uploads-ssl.webflow.com/615d8a846a58e267138fd5c9/615d8a846a58e2b6ca8fd622_angular-p-500.png")]
        [TestCase(@"\tata\toto.txt")]
        [TestCase(@"tata\toto.txt")]
        public void CheckDistant(string path)
        {
            
        }
        
        
    }

    public static class UriCheck
    {
        public static bool CheckUri(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute))
        }
        
    }
}