/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Microsoft.Scripting.Actions {
    public class FieldTracker : MemberTracker {
        private FieldInfo _field;

        public FieldTracker(FieldInfo field) {
            _field = field;
        }

        public override Type DeclaringType {
            get { return _field.DeclaringType; }
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Field; }
        }

        public override string Name {
            get { return _field.Name; }
        }

        public bool IsPublic {
            get {
                return _field.IsPublic;
            }
        }

        public bool IsInitOnly {
            get {
                return _field.IsInitOnly;
            }
        }

        public Type FieldType {
            get {
                return _field.FieldType;
            }
        }

        public bool IsStatic {
            get {
                return _field.IsStatic;
            }
        }

        public FieldInfo Field {
            get {
                return _field;
            }
        }

        public override string ToString() {
            return _field.ToString();
        }
    }
}
