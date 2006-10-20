/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace IronPythonTest {

    public interface IExplicitTest1 {
        string A();
        string B();
        string C();
        string D();
    }

    public interface IExplicitTest2 {
        string A();
        string B();
    }

    public interface IExplicitTest3 {
        int M();
    }

    public interface IExplicitTest4 {
        int M(int i);
    }

    public class ExplicitTest : IExplicitTest1, IExplicitTest2 {
        #region IExplicitTest1 Members
        string IExplicitTest1.A() {
            return "ExplicitTest.IExplicitTest1.A";
        }
        string IExplicitTest1.B() {
            return "ExplicitTest.IExplicitTest1.B";
        }
        string IExplicitTest1.C() {
            return "ExplicitTest.IExplicitTest1.C";
        }
        public string D() {
            return "ExplicitTest.D";
        }
        #endregion

        #region IExplicitTest2 Members
        string IExplicitTest2.A() {
            return "ExplicitTest.IExplicitTest2.A";
        }
        public string B() {
            return "ExplicitTest.B";
        }
        #endregion
    }

    public class ExplicitTestArg : IExplicitTest3, IExplicitTest4 {
        #region IExplicitTest3 Members
        int IExplicitTest3.M() {
            return 3;
        }
        #endregion

        #region IExplicitTest4 Members
        int IExplicitTest4.M(int i) {
            return 4;
        }
        #endregion
    }
}
