using System.Runtime.CompilerServices;

// in standard .Net, the info assembly is by default generated on the fly. This behavior can be defended and therefore,
// for internalVisibleTo, it is important to put them in a separate class
[assembly: InternalsVisibleTo("PRF.Utils.Tracer.UnitTest")]
