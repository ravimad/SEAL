using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WeakUpdateTest
{
    public static void Main()
    {
        Temp a = getANewTempClass();
        Temp b = getANewTempClass();
        a = b;
        Temp c = a;
    }

    public static Temp getANewTempClass()
    {
        Temp x = new Temp();
        return x;
    }
}

public class Temp
{

}

