using System;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions
{
    [TestFixture]
    internal sealed class MathExtTests
    {
        [Test]
        public void Max_return_maximum_value()
        {
            //Arrange
            var tmp1 = new TmpClass(1);
            var tmp2 = new TmpClass(2);

            //Act
            var res = MathExt.Max(tmp1, tmp2);

            //Assert
            Assert.AreSame(tmp2, res);
        }
        
        [Test]
        public void Min_return_maximum_value()
        {
            //Arrange
            var tmp1 = new TmpClass(1);
            var tmp2 = new TmpClass(2);

            //Act
            var res = MathExt.Min(tmp1, tmp2);

            //Assert
            Assert.AreSame(tmp1, res);
        }

        private sealed class TmpClass : IComparable
        {
            private readonly int _number;

            public TmpClass(int number)
            {
                _number = number;
            }
            public int CompareTo(object obj)
            {
                if (obj is TmpClass tmp)
                {
                    return _number.CompareTo(tmp._number);
                }

                throw new ArgumentOutOfRangeException();
            }
        }
        
        [Test]
        public void Clamp_double_return_clamped_value()
        {
            //Arrange

            //Act
            var res = MathExt.Clamp(20d, 2d, 18d);

            //Assert
            Assert.AreEqual(18d, res);
        }

        [Test]
        public void Clamp_double_return_clamped_min_value()
        {
            //Arrange

            //Act
            var res = MathExt.Clamp(1d, 2d, 18d);

            //Assert
            Assert.AreEqual(2d, res);
        }


        [Test]
        public void Clamp_double_return_initial_value_if_within_interval()
        {
            //Arrange

            //Act
            var res = MathExt.Clamp(20d, 2d, 30d);

            //Assert
            Assert.AreEqual(20d, res);
        }
        
        [Test]
        public void Clamp_int_return_clamped_max_value()
        {
            //Arrange

            //Act
            var res = MathExt.Clamp(20, 2, 18);

            //Assert
            Assert.AreEqual(18, res);
        }

        [Test]
        public void Clamp_int_return_clamped_min_value()
        {
            //Arrange

            //Act
            var res = MathExt.Clamp(1, 2, 18);

            //Assert
            Assert.AreEqual(2, res);
        }

        [Test]
        public void Clamp_int_return_initial_value_if_within_interval()
        {
            //Arrange

            //Act
            var res = MathExt.Clamp(20, 2, 30);

            //Assert
            Assert.AreEqual(20, res);
        }
        
        [Test]
        public void Clamp_IComparable_return_clamped_value()
        {
            //Arrange
            var tmp1 = new TmpClass(1);
            var tmp2 = new TmpClass(2);
            var valToTest = new TmpClass(3);

            //Act
            var res = valToTest.Clamp(tmp1, tmp2);

            //Assert
            Assert.AreSame(tmp2, res);
        }


        [Test]
        public void Clamp_IComparable__return_initial_value_if_within_interval()
        {
            //Arrange
            var tmp1 = new TmpClass(1);
            var tmp2 = new TmpClass(3);
            var valToTest = new TmpClass(2);

            //Act

            //Act
            var res = valToTest.Clamp(tmp1, tmp2);

            //Assert
            Assert.AreSame(valToTest, res);
        }
    }
}