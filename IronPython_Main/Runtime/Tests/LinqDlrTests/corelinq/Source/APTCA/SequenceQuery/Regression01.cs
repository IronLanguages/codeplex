extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;

namespace APTCATest
{
    public partial class AccessMembersTests
    {
        /// <summary>
        /// Regression Test Case for DDB#82999
        /// </summary>
        [APTCATest]
        static public int DDB82999_QueryGroup()
        {
//TODO: create IQueryable, etc so we don't depend on system.core anymore.
            /*AltCore.IQueryable<int> q = (new[] { 7, 8, 5, 7, 6, }).AsQueryable();
            Expression<Func<int, int>> ex1 = x1 => 2;
            Expression<Func<int, int>> ex2 = x1 => 3;
            Expression<Func<AltCore.IGrouping<int, int>, int>> ex3 = x3 => (x3.AsQueryable()).Min();
            var g = AltCore.Queryable.GroupBy<int, int, int>(q, ex1, ex2);
            var qiq = AltCore.Queryable.Select<AltCore.IGrouping<int, int>, int>(g, ex3);
            var e3 = qiq.GetEnumerator();
            try
            {
                var e3HasNext = e3.MoveNext();
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.SeqQuery);
            }*/

            return 0;
        }
    }
}
