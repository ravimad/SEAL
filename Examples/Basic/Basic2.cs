using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Basic2
{
    int a;
    Basic b2;
    Basic2 b3;

    public void foo4()
    {
        b2 = new Basic();
        b3 = new Basic2();
        a = foo5(b3);

        Basic b4 = foo6(b3);
    }

    public int foo5(Basic2 b)
    {
        Basic2 c = b;
        foo6(c);
        return b.a;
    }

    public Basic foo6(Object c)
    {
        Basic x = (Basic)c;
        return x;

    }
}
