using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ListExample2
{
    
    public static void Main()
    {
        customListObject obj = new customListObject(10, "abc");
        customList l = new customList(obj);
        
        customListObject obj2 = new customListObject(20, "efg");
        customListObject obj3 = new customListObject(30, "hij");

        //l.addelement(obj2);
        l.addelement(obj3);

        customListObject item = l.getelement();
        int a = item.getint();
    }
}

public class customListObject
{
    int x;
    string b;

    public customListObject(int a,string s)
    {
        this.x = a;
        this.b = s;
    }

    public int getint()
    {
        return x;
    }
}

public class customList
{
    customListObject head;
    customList next;

    public customList(customListObject x)
    {
        head = x;
        next = null;
    }

    public customList(customListObject x, customList y)
    {
        head = x;
        next = y;
    }

    public void addelement (customListObject x)
    {
        this.next = new customList(this.head, this.next);
        this.head = x;
    }

    public customListObject getelement ()
    {
        return this.head;
    }
}
