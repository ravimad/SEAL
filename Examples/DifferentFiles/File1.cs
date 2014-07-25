using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Class1
{
    public Class1 c1;
    Temp t;
    int a;
    
    public static void Main()
    {
        Class1 first = new Class1();
        first.c1 = new Class1();
        first.c1.c1 = new Class1();

        first.c1 = first;
        first.c1.c1 = first;
        first = new Class1();
    }

    public static Class1 getX(Class1 a)
    {
        return a.c1;
    }

    public void setX(Class1 a)
    {
        this.c1 = a;
    }

}

