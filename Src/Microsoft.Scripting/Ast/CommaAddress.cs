using System.Collections.Generic;

namespace Microsoft.Scripting.Ast {
    class CommaAddress : EvaluationAddress {
        private List<EvaluationAddress> _addrs;

        public CommaAddress(Block address, List<EvaluationAddress> addresses)
            : base(address) {
            _addrs = addresses;
        }

        public override object GetValue(CodeContext context, bool outParam) {
            object result = null;
            for (int i = 0; i < _addrs.Count; i++) {
                EvaluationAddress current = _addrs[i];

                if (current != null) {
                    object val = current.GetValue(context, outParam);
                    if (i == Index) {
                        result = val;
                    }
                }
            }
            return result;
        }

        public override object AssignValue(CodeContext context, object value) {
            EvaluationAddress addr = _addrs[Index];
            if (addr != null) return addr.AssignValue(context, value);
            return null;
        }

        private int Index {
            get {
                return ((Block)Expression).Expressions.Count - 1;
            }
        }
    }
}
