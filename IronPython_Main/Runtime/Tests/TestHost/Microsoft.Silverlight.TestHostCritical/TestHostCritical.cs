using System.Security;

namespace Microsoft.Silverlight.TestHostCritical {
    [SecuritySafeCritical]
    public static class Environment {
        public static int ProcessorCount {
            get {
                return System.Environment.ProcessorCount;
            }
        }
    }
}
