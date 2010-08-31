using System;
using System.Linq.Expressions;

public class C
{
    public static void M(int x1, int x2, int x3, int x4, int x5) { }

    static int Main()
    {
	return 0;
//TODO: evaluate if we can get ms.scripting to trigger linq features here.
/*        try
        {
            Expression<Action<int, int, int, int, int>> e = (x1, x2, x3, x4, x5) => M(x1, x2, x3, x4, x5);
            e.Compile()(0, 0, 0, 0, 0);
        }
        catch (ArgumentException)
        {
            return 0;
        }
        return 1;*/
    }
}
