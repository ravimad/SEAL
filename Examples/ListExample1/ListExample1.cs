using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ListExample1
{
    
    private static List<ListObject> l;


    public static void addelement(ListObject x)
    {
        l.Add(x);
    }

    public static ListObject getelement()
    {
        return l.First();
    }

    public static void Main()
    {
        l = new List<ListObject>();
        ListObject x = new ListObject();
        addelement(x);
        ListObject y = new ListObject();
        addelement(y);

        ListObject z = getelement();
        
    }
}

public class ListObject
{

}
