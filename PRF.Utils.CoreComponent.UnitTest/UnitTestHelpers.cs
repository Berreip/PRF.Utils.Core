using System.Runtime.InteropServices;

namespace PRF.Utils.CoreComponent.UnitTest;

public static class UnitTestHelpers
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}