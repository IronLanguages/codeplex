using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// Wrapper exception used for when the user wants to raise a string as an exception.
    /// </summary>
    [Serializable]
    public sealed class StringException : Exception, IPythonException {
        object value;

        public StringException() { }

        public StringException(string message)
            : base(message) {
            value = message;
        }

        public StringException(string name, object value)
            : base(name) {
            this.value = value;
        }

        public StringException(string message, Exception innerException)
            : base(message, innerException) {
        }

#if !SILVERLIGHT // SerializationInfo
        private StringException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            value = info.GetValue("value", typeof(object));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("value", value);

            base.GetObjectData(info, context);
        }
#endif

        public override string ToString() {
            return base.Message;
        }

        public object Value {
            get {
                return value;
            }
        }

        #region IPythonException Members

        public object ToPythonException() {
            return this;
        }

        #endregion
    }
}
