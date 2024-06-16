namespace CommonUnitTest;

public static class UnitTestFolder
{
    public static DirectoryInfo Get(string additionalPart)
    {
        var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        return new DirectoryInfo(Path.Combine(Path.GetDirectoryName(assemblyPath)!, additionalPart));
    }
}