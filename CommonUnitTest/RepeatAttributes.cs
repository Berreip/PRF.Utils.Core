using System.Reflection;
using Xunit.Sdk;

namespace CommonUnitTest;

/// <summary>
/// Repeat attribute is used with an int iteration in test signature:
/// <example>
/// [Theory]
/// [Repeat(10)]
/// public void Test_that_smthg(int iteration) {...}
/// </example>
/// </summary>
public class RepeatAttribute : DataAttribute
{
    private readonly int _count;

    public RepeatAttribute(int count)
    {
        _count = count;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return Enumerable.Range(1, _count).Select(i => new object[] { i });
    }
}