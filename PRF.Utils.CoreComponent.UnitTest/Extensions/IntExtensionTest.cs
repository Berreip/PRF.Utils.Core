using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions
{
    [TestFixture]
    public class IntExtensionTest
    {
        [Test]
        public void IsInRangeV1()
        {
            //Configuration
            const int nb = 45;

            //Test
            var res = nb.IsInRange(40, 50);

            //Verify
            Assert.IsTrue(res);
        }

        [Test]
        public void IsInRangeV2()
        {
            //Configuration
            const int nb = 35;

            //Test
            var res = nb.IsInRange(40, 50);

            //Verify
            Assert.IsFalse(res);
        }

    }
}
