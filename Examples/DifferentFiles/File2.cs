using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Class2
{
    Class1 c1;
    Class2 c2;
    Temp t;
    int a;

    public static void Main()
    {
        Class2 x = new Class2();
        x.c1 = new Class1();

        Class1 temp = new Class1();

        x.c1.setX(temp);

        Class1 output = Class1.getX(x.c1);

        convertObjectToClass2(x);
        convertObjectToClass2(output);

    }

    public static void convertObjectToClass2 (Object o)
    {
        Class2 conv = (Class2)o;
        return;
    }

}

public class Temp
{

}
