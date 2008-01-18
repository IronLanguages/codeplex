using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// GeneratorExitException is a standard exception raised by Generator.Close() to allow a caller
    /// to close out a generator.
    /// </summary>
    /// <remarks>GeneratorExit is introduced in Pep342 for Python2.5. </remarks>
    [Serializable]
    public sealed class GeneratorExitException : Exception {
        public GeneratorExitException() {
        }

        public GeneratorExitException(string message)
            : base(message) {
        }
        public GeneratorExitException(string message, Exception innerException)
            : base(message, innerException) {
        }

#if !SILVERLIGHT // SerializationInfo
        private GeneratorExitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
