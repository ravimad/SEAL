using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Basic
{
    int a;
    Basic b2;
    Basic2 b3;

    public void foo1()
    {
        b2 = new SuperClass();
        b3 = new Basic2();
        a = foo2(b2);

        Basic b4 = foo3(b2);
    }

    public int foo2(Basic b)
    {
        Basic c = b.b2;
        foo3(c);
        return b.a;
    }

    public Basic foo3(Object c)
    {
        Basic x = (Basic)c;
        return x;
    }

}

public class SuperClass : Basic
{

}
