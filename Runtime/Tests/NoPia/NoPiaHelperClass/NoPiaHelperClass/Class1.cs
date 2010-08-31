using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoPiaHelperClass
{
    public class NoPiaHelper
    {
        static public FooStruct X;

        static public Type FooStructType = typeof(FooStruct);

        static public FooStruct Add(FooStruct arg1, FooStruct arg2)
        {
            return new FooStruct();
        }

        static public FooStruct Not(FooStruct arg1)
        {
            return new FooStruct();
        }

        static public Type ISubFuncPropType = typeof(ISubFuncProp);

        static public bool Equal(FooStruct arg1, FooStruct arg2)
        {
            return true;
        }
    }
}
