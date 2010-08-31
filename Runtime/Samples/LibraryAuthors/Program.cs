using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Dynamic;
using System.Linq.Expressions;
using System.ComponentModel;

namespace Bags
{
    public class Program
    {
        static void Main(string[] args)
        {
            dynamic bagA = new FastNBag(5);
            bagA.Foo = 1;
            bagA.FooFoo = 2;
            bagA.FooBar = 3;
            bagA.Bar = 4;
            bagA.BarFoo = 5;
            bagA.BarBar = 6;

            // Retrieves from fast array:
            Console.WriteLine(bagA.FooBar);

            // Retrieves from hashtable:
            Console.WriteLine(bagA.BarBar);

            // Binding fails - C#'s own runtime binder exception is thrown:
            try
            {
                Console.WriteLine(bagA.Baz);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }



    public class FastNBag : IDynamicMetaObjectProvider, INotifyPropertyChanged
    {
        private object[] fastArray;
        private Dictionary<string, int> fastTable;

        private Dictionary<string, object> hashTable
            = new Dictionary<string, object>();

        private readonly int fastCount;

        public int Version { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public FastNBag(int fastCount)
        {
            this.fastCount = fastCount;
            this.fastArray = new object[fastCount];
            this.fastTable = new Dictionary<string, int>(fastCount);
        }

        public bool TryGetValue(string key, out object value)
        {
            int index = GetFastIndex(key);
            if (index != -1)
            {
                value = GetFastValue(index);
                return true;
            }
            else if (fastTable.Count == fastCount)
            {
                return hashTable.TryGetValue(key, out value);
            }
            else
            {
                value = null;
                return false;
            }
        }

        public void SetValue(string key, object value)
        {
            int index = GetFastIndex(key);
            if (index != -1)
                SetFastValue(index, value);
            else
                if (fastTable.Count < fastCount)
                {
                    index = fastTable.Count;
                    fastTable[key] = index;
                    SetFastValue(index, value);
                }
                else
                    hashTable[key] = value;

            Version++;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(key));
            }
        }

        public object GetFastValue(int index)
        {
            return fastArray[index];
        }

        public void SetFastValue(int index, object value)
        {
            fastArray[index] = value;
        }

        public int GetFastIndex(string key)
        {
            int index;
            if (fastTable.TryGetValue(key, out index))
                return index;
            else
                return -1;
        }

        public IEnumerable<string> GetKeys()
        {
            var fastKeys = fastTable.Keys;
            var hashKeys = hashTable.Keys;

            var keys = fastKeys.Concat(hashKeys);

            return keys;
        }

        public bool CheckVersion(int ruleVersion)
        {
            return (Version == ruleVersion);
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new MetaFastNBag(parameter, this);
        }



        private class MetaFastNBag : DynamicMetaObject
        {
            public MetaFastNBag(Expression expression, FastNBag value)
                : base(expression, BindingRestrictions.Empty, value) { }

            public override DynamicMetaObject BindGetMember
                    (GetMemberBinder binder)
            {
                var self = this.Expression;
                var bag = (FastNBag)base.Value;

                int index = bag.GetFastIndex(binder.Name);

                Expression target;

                // If match found in fast array:
                if (index != -1)
                {
                    // Fetch result from fast array.
                    target =
                        Expression.Call(
                            Expression.Convert(self, typeof(FastNBag)),
                            typeof(FastNBag).GetMethod("GetFastValue"),
                            Expression.Constant(index)
                        );
                }
                // Else, if no match found in fast array, but fast array is full:
                else if (bag.fastTable.Count == bag.fastCount)
                {
                    // Fetch result from dictionary.
                    var keyExpr = Expression.Constant(binder.Name);
                    var valueExpr = Expression.Variable(typeof(object));

                    var dictCheckExpr =
                        Expression.Call(
                            Expression.Convert(self, typeof(FastNBag)),
                            typeof(FastNBag).GetMethod("TryGetValue"),
                            keyExpr,
                            valueExpr
                        );
                    var dictFailExpr =
                        Expression.Block(
                            binder.FallbackGetMember(this).Expression,
                            Expression.Default(typeof(object))
                        );

                    target =
                        Expression.Block(
                            new[] { valueExpr },
                            Expression.Condition(
                                dictCheckExpr,
                                valueExpr,
                                dictFailExpr
                            )
                        );
                }
                // Else, no match found in fast array, fast array is not yet full:
                else
                {
                    // Fail binding, but only until fast array is updated.
                    var versionCheckExpr =
                        Expression.Call(
                            Expression.Convert(self, typeof(FastNBag)),
                            typeof(FastNBag).GetMethod("CheckVersion"),
                            Expression.Constant(bag.Version)
                        );
                    var versionMatchExpr =
                        binder.FallbackGetMember(this).Expression;
                    var updateExpr =
                        binder.GetUpdateExpression(versionMatchExpr.Type);

                    target =
                        Expression.Condition(
                            versionCheckExpr,
                            versionMatchExpr,
                            updateExpr
                        );
                }

                var restrictions = BindingRestrictions
                                       .GetInstanceRestriction(self, bag);

                return new DynamicMetaObject(target, restrictions);
            }


            public override DynamicMetaObject BindSetMember(
                SetMemberBinder binder, DynamicMetaObject value)
            {
                var self = this.Expression;

                var keyExpr = Expression.Constant(binder.Name);
                var valueExpr = Expression.Convert(
                                    value.Expression,
                                    typeof(object)
                                );

                var target = Expression.Block(
                    Expression.Call(
                        Expression.Convert(self, typeof(FastNBag)),
                        typeof(FastNBag).GetMethod("SetValue"),
                        keyExpr,
                        valueExpr
                    ),
                    valueExpr);

                var restrictions = BindingRestrictions
                                      .GetTypeRestriction(self, typeof(FastNBag));

                return new DynamicMetaObject(target, restrictions);
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                var bag = (FastNBag)base.Value;

                return bag.GetKeys();
            }
        }
    }
}
