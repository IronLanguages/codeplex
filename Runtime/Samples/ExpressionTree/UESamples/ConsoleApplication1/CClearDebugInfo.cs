using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples
{
    class CClearDebugInfo
    {
        //ClearDebugInfo(SymbolDocumentInfo)
        static public void ClearDebugInfo1()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This SymbolDocumentInfo represents the source file that resulted in the expressions marked with the DebugInfo.
            SymbolDocumentInfo DocInfo = Expression.SymbolDocument("FakeSourceFile.Fake");

            
            //This Expression represents the end of a section identified by a previous DebugInfo
            DebugInfoExpression MyClearDebug = Expression.ClearDebugInfo(
                                    DocInfo
                                );

            //To demonstrate the use of ClearDebugInfo, we create a block with several expressions, some of which are
            //marked by by a sequence of a DebugInfo and a ClearDebugInfo.
            //In this case, we've marked the expression that creates a constant with value 6 to have been generated from source
            //in the file 'FakeSourceFile.Fake', on line 5, between columns 1 and 10. The other expressions will not get associated
            //debug info (they might have been, for example, compiler generated code).
            Expression MyBlock = Expression.Block(
                                    Expression.Constant(5),
                                    Expression.DebugInfo(DocInfo,5,1,5,10),
                                    Expression.Constant(6),
                                    MyClearDebug,
                                    Expression.Constant(7)
                                );
            

            //</Snippet1>
        }
    }
}
