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
        #region "Class Constructors - IQueryable Create with Sequence"
        /// <summary>
        /// Hit SequenceQuery: IQueryable Create(Type elementType, Expression expression)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassDefaultCtor()
        {
            List<DefaultCtorClass> objList = new List<DefaultCtorClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            DefaultCtorClass[] objAry = new DefaultCtorClass[9];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPublicCtor()
        {
            List<PublicCtorClass> objList = new List<PublicCtorClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            PublicCtorClass[] objAry = new PublicCtorClass[99];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPublicParaCtor()
        {
            List<PublicParaCtorClass> objList = new List<PublicParaCtorClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            PublicParaCtorClass[] objAry = new PublicParaCtorClass[999];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPublic2Ctors()
        {
            List<Public2CtorsClass> objList = new List<Public2CtorsClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            Public2CtorsClass[] objAry = new Public2CtorsClass[9999];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPublicCtorsSub()
        {
            List<PublicCtorsSubClass> objList = new List<PublicCtorsSubClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            PublicCtorsSubClass[] objAry = new PublicCtorsSubClass[99999];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns></returns>
        [APTCATest]
        static public int ClassProtectedCtor()
        {
            List<ProtectedCtorClass> objList = new List<ProtectedCtorClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            ProtectedCtorClass[] objAry = new ProtectedCtorClass[9999];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassInternalCtor()
        {
            List<InternalCtorClass> objList = new List<InternalCtorClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            InternalCtorClass[] objAry = new InternalCtorClass[9999];
            ie = objAry;
            iq = ie.AsQueryable();

            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPrivateCtor()
        {
            List<PrivateCtorClass> objList = new List<PrivateCtorClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            PrivateCtorClass[] objAry = new PrivateCtorClass[999];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPrivate2Ctors()
        {
            List<Private2CtorsClass> objList = new List<Private2CtorsClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            Private2CtorsClass[] objAry = new Private2CtorsClass[99];
            ie = objAry;
            iq = ie.AsQueryable();

            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassStaticCtor()
        {
            List<StaticCtorClass> objList = new List<StaticCtorClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            StaticCtorClass[] objAry = new StaticCtorClass[9];
            ie = objAry;
            iq = ie.AsQueryable();

            return 0;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassStatic2Ctors()
        {
            List<Static2CtorsClass> objList = new List<Static2CtorsClass>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();
            Static2CtorsClass[] objAry = new Static2CtorsClass[999];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }
#if false
        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassProtectedCtor()
        {
            List<ProtectedCtorClass> objList = new List<ProtectedCtorClass>();
            IEnumerable ie = objList;
            try
            {
                AltCore.IQueryable iq = ie.AsQueryable();

                ProtectedCtorClass[] objAry = new ProtectedCtorClass[9999];
                ie = objAry;
                try
                {
                    iq = ie.AsQueryable();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.SeqQuery);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.SeqQuery);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassInternalCtor()
        {
            List<InternalCtorClass> objList = new List<InternalCtorClass>();
            IEnumerable ie = objList;
            try
            {
                AltCore.IQueryable iq = ie.AsQueryable();

                InternalCtorClass[] objAry = new InternalCtorClass[9999];
                ie = objAry;
                try
                {
                    iq = ie.AsQueryable();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.SeqQuery);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.SeqQuery);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPrivateCtor()
        {
            List<PrivateCtorClass> objList = new List<PrivateCtorClass>();
            IEnumerable ie = objList;
            try
            {
                AltCore.IQueryable iq = ie.AsQueryable();

                PrivateCtorClass[] objAry = new PrivateCtorClass[999];
                ie = objAry;
                try
                {
                    iq = ie.AsQueryable();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.SeqQuery);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.SeqQuery);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassPrivate2Ctors()
        {
            List<Private2CtorsClass> objList = new List<Private2CtorsClass>();
            IEnumerable ie = objList;
            try
            {
                AltCore.IQueryable iq = ie.AsQueryable();

                Private2CtorsClass[] objAry = new Private2CtorsClass[99];
                ie = objAry;
                try
                {
                    iq = ie.AsQueryable();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.SeqQuery);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.SeqQuery);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassStaticCtor()
        {
            List<StaticCtorClass> objList = new List<StaticCtorClass>();
            IEnumerable ie = objList;
            try
            {
                AltCore.IQueryable iq = ie.AsQueryable();

                StaticCtorClass[] objAry = new StaticCtorClass[9];
                ie = objAry;
                try
                {
                    iq = ie.AsQueryable();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.SeqQuery);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.SeqQuery);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int ClassStatic2Ctors()
        {
            List<Static2CtorsClass> objList = new List<Static2CtorsClass>();
            IEnumerable ie = objList;
            try
            {
                AltCore.IQueryable iq = ie.AsQueryable();

                Static2CtorsClass[] objAry = new Static2CtorsClass[999];
                ie = objAry;
                try
                {
                    iq = ie.AsQueryable();
                }
                catch (Exception ex)
                {
                    return ExceptionHandler(ex, ExceptionSource.SeqQuery);
                }
            }
            catch (Exception ex)
            {
                return ExceptionHandler(ex, ExceptionSource.SeqQuery);
            }
            return 0;
        }
#endif
        #endregion

        #region "Struct Constructors - IQueryable Create with Sequence"
        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int StructDefaultCtor()
        {
            List<DefaultCtorStruct> objList = new List<DefaultCtorStruct>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            DefaultCtorStruct[] objAry = new DefaultCtorStruct[8];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int StructPublicParaCtor()
        {
            List<PublicParaCtorStruct> objList = new List<PublicParaCtorStruct>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            PublicParaCtorStruct[] objAry = new PublicParaCtorStruct[88];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int StructInternalCtor()
        {
            List<InternalCtorStruct> objList = new List<InternalCtorStruct>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            InternalCtorStruct[] objAry = new InternalCtorStruct[888];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        /// <summary>
        /// IQueryable Create(Type elementType, IEnumerable sequence)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int StructPrivateCtor()
        {
            List<PrivateCtorStruct> objList = new List<PrivateCtorStruct>();
            IEnumerable ie = objList;
            AltCore.IQueryable iq = ie.AsQueryable();

            PrivateCtorStruct[] objAry = new PrivateCtorStruct[88];
            ie = objAry;
            iq = ie.AsQueryable();
            return 0;
        }

        #endregion

        #region "IQueryable Create with Expression"
        /// <summary>
        /// ??? IQueryable Create(Type elementType, Expression expression)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int CreateQueryWithExpression()
        {
            // TODO:
            return 0;
        }
        #endregion

        #region "SequenceExecutor Create with Expression"
        /// <summary>
        /// internal static SequenceExecutor Create(Expression expression)
        /// </summary>
        /// <returns>0 PASS, otherwise FAILed</returns>
        [APTCATest]
        static public int LocaIntArray()
        {
//TODO: create own Enumerable, IQueryable so this scenario can be enabled.
/*            int[] s1 = { -4, 3, 3, -10, 10 };
            MethodInfo mi = typeof(AltCore.Enumerable).GetMethod("Min", new Type[] { typeof(IEnumerable<int>) });

            AltCore.IQueryable s2 = s1.AsQueryable();
            int r1 = s1.Min<int>();
            int r2 = (int)s2.Provider.Execute(Expression.Call(null, mi, new Expression[] { s2.Expression }));
*/
            return 0;
        }
        #endregion

    }
}
