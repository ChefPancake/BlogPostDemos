using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiscriminatedUnionDemo.StaticCtorsExts;
using static DiscriminatedUnionDemo.WithStaticCtors;

namespace DiscriminatedUnionTests;

[TestClass]
public class StaticCtorsTests {
    [TestMethod]
    public void DivideByNonZero_IsSome() {
        int dividend = 16;
        int divisor = 3;

        Option<int> quotientOption = dividend.DivideBy(divisor);

        Assert.AreEqual(quotientOption.HasValue, true);
        Assert.AreEqual(quotientOption.Value, 5);
    }

    [TestMethod]
    public void DivideByZero_IsNone() {
        int dividend = 16;
        int divisor = 0;

        Option<int> quotientOption = dividend.DivideBy(divisor);

        Assert.AreEqual(quotientOption.HasValue, false);
        Assert.AreEqual(quotientOption.Value, default);
    }
}
