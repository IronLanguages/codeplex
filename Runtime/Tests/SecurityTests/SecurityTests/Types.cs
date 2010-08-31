using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using System.Security;
using System.Security.Permissions;

// contains custom Types to be used with AppDomain tests
// either for instantiating in different AppDomains or doing
// cross AppDomain calls

namespace SecurityTests
{
    [Serializable]
    public class TestClass : MarshalByRefObject {
        private bool myPrivateField;

        // requires homogenous AppDomain
        public void DoDynamic() {
            dynamic x = "hello";
            x.ToUpper();
        }

        // requires reflection permissions
        public void DoTrustedOperation() {
            Expression e = Expression.Field(Expression.Constant(this), "myPrivateField");
            // On .NET 3.5 and below .Compile() will throw an exception here and never get to invoke.
            var lm = Expression.Lambda(e).Compile();
            // On .NET 4.0 a tree accessing a private field can be compiled but will throw on invocation.
            try
            {
                lm.DynamicInvoke();
            }
            catch (System.Reflection.TargetInvocationException ex) // 3.5 throws SecurityException so this keeps the scenario code looking simpler
            {
                if (ex.InnerException is FieldAccessException)
                {
                    throw new SecurityException("Caught FieldAccessException as expected", ex);
                }
                else
                {
                    throw;
                }
            }

        }

        // requires homogenous AppDomain
        public void AccessIDO() {
            dynamic x = new System.Dynamic.ExpandoObject();
            x.Data = "hello";
        }
    }

    #region LINQ test types
    // for LINQ to SQL tests
    [Table(Name = "Customers")]
    public class Customer {
        [Column(IsPrimaryKey = true)]
        public string CustomerID;
        [Column]
        public string City;
    }

    public partial class Northwind : DataContext {
        public Table<Customer> Customers;
        public Northwind(string connection) : base(connection) { }
    }

    [Serializable]
    class LinqTestClass : MarshalByRefObject, IDisposable
    {
        Northwind db;

        public LinqTestClass()
        {
            // remote test DB
            db = new Northwind("Data Source=VBSQL2008B;Initial Catalog=NorthwindReadOnly;User ID=sa;password=Admin_007;");
        }

        public void DoQuery()
        {
        var query =
            from c in db.Customers
            where c.City == "London"
            select c;

        try
        {
            foreach (var res in query)
            {
                string total = res.ToString();
            }
        }
        catch (System.Data.SqlClient.SqlException)
        {
            Console.WriteLine("Failed executing against SQL DB in LinqTestClass.");
            throw;
        }
        }
  

        // Attempt a LINQ to SQL query passing Expression nodes that LINQ provider doesn't understand (throws ArgumentException)
        public void DoInvalidQuery() {
            var cust = Expression.Variable(typeof(Customer), "myCust");
            Expression<Func<Customer, bool>> predicate =
                Expression.Lambda<Func<Customer, bool>>(
                    Expression.TryCatch(
                        Expression.Condition(
                            Expression.Equal(Expression.Constant(1), Expression.Constant(1)),
                            Expression.Constant(true),
                            Expression.Constant(false)
                        ),
                        Expression.Catch(typeof(DivideByZeroException), Expression.Constant(false))
                    ),
                    cust
                );

            var query = db.Customers.Where(predicate);

            try {
                foreach (var res in query) {
                    Console.WriteLine(res.CustomerID);
                }
            } catch (System.Data.SqlClient.SqlException) {
                Console.WriteLine("Failed executing against SQL DB in LinqTestClass.");
                throw;
            }
        }

        public void Dispose() {
            db.Dispose();
        }
    }
    #endregion

    #region Custom IDO types
    // from SiteTest.Actions.TestDynamicObject.cs
    [Serializable]
    public class MBRODynamicObject : MarshalByRefObject, IDynamicMetaObjectProvider {
        public string Data { get; set; }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new MBROBinder(parameter, BindingRestrictions.Empty, this);
        }

        public object GetComObj() {
            return Activator.CreateInstance<DlrComLibraryLib.PropertiesClass>();
        }
    }

    class MBROBinder : DynamicMetaObject {
        public MBROBinder(Expression expression, BindingRestrictions restrictions, object value) :
            base(expression, restrictions, value) {
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            return new DynamicMetaObject(
                Expression.Constant("MBRO_GetMember"),
                BindingRestrictions.Empty
            );
        }
    }

    #endregion
}
