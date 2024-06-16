using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;


public class IntExtensionTest
{
    [Fact]
    public void IsInRangeV1()
    {
        //Configuration
        const int nb = 45;

        //Test
        var res = nb.IsInRange(40, 50);

        //Verify
        Assert.True(res);
    }

    [Fact]
    public void IsInRangeV2()
    {
        //Configuration
        const int nb = 35;

        //Test
        var res = nb.IsInRange(40, 50);

        //Verify
        Assert.False(res);
    }

}