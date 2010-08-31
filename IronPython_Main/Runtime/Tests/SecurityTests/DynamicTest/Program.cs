using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicSecurityTest {
    class Program {
        static void Main(string[] args) {
            dynamic x = new MBRODynamicObjectFileWriter();
            var result = x.Data;
        }
    }

    [Serializable]
    // A simple IDMOP which performs privileged operations (file system manipulation) in its binder
    class MBRODynamicObjectFileWriter : MarshalByRefObject, IDynamicMetaObjectProvider {
        public string Data { get; set; }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new MBROWriterBinder(parameter, BindingRestrictions.Empty, this);
        }

        public object GetComObj() {
            return Activator.CreateInstance(Type.GetTypeFromProgID("DlrComLibrary.Properties"));
        }
    }

    class MBROWriterBinder : DynamicMetaObject {
        public MBROWriterBinder(Expression expression, BindingRestrictions restrictions, object value) :
            base(expression, restrictions, value) {
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\test.txt";
            var f = System.IO.File.OpenWrite(path);
            f.WriteByte(65);
            f.Close();
            System.IO.File.Delete(path);

            return new DynamicMetaObject(
                Expression.Constant("MBRO_GetMember"),
                BindingRestrictions.GetExpressionRestriction(Expression.Constant(true))
            );
        }
    }
}
