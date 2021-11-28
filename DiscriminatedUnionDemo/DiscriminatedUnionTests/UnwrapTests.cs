using DiscriminatedUnionDemo;
using DiscriminatedUnionDemo.UnwrapExts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static DiscriminatedUnionDemo.WithUnwrap;

namespace DiscriminatedUnionTests {
    [TestClass]
    public class UnwrapTests {
        [TestMethod]
        public void DivideByNonZero_IsSome() {
            int dividend = 16;
            int divisor = 3;
            bool wasTested = false;

            Option<int> quotientOption = dividend.DivideBy(divisor);

            quotientOption.Unwrap(
                x => {
                    Assert.AreEqual(5, x);
                    wasTested = true;
                },
                () => Assert.Fail());
            Assert.IsTrue(wasTested);
        }

        [TestMethod]
        public void DivideByZero_IsNone() {
            int dividend = 16;
            int divisor = 0;
            bool wasTested = false;

            Option<int> quotientOption = dividend.DivideBy(divisor);

            quotientOption.Unwrap(
                x => Assert.Fail(),
                () => wasTested = true);
            Assert.IsTrue(wasTested);
        }

        [TestMethod]
        public void AddThenDivideThenSubtractThenDivide_IsSome() {
            int baseValue = 50;
            int addByValue = 10;
            int divideByValue = 4;
            int subtractByValue = 2;
            int expected = 3;

            Option<int> resultOption =
                baseValue
                .AddBy(addByValue)
                .DivideBy(divideByValue)
                .OptionSubtractBy(subtractByValue)
                .OptionDivideBy(divideByValue);

            resultOption.Unwrap(
                x => Assert.AreEqual(expected, x),
                () => Assert.Fail());
        }
    }
}
