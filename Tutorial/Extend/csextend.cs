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
using System.Collections;

public delegate int Transformer(int input);

public class Simple : IEnumerable {

    private int data;

    public Simple(int data) {
        this.data = data;
    }

    public override string ToString() {
        return String.Format("Simple<{0}>", data);
    }

    public IEnumerator GetEnumerator() {
        for (int i = 0; i < data; i ++) {
            yield return new Simple(i);
        }
    }

    public int Transform(Transformer t) {
        return t(data);
    }

    public static Simple operator +(Simple a, Simple b) {
        return new Simple(a.data + b.data);
    }
}
