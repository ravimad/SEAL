using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Animal
{

}

public class Cat : Animal
{

}

public class Check
{

    public static void Main()
    {
        Object c = new Cat();
        Cat b = c as Cat;
        Animal d = c as Animal;
        Cat e = (Cat)d;
    }
}