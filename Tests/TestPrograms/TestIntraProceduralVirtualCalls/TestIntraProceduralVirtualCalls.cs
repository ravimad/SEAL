using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIntraProceduralVirtualCalls
{
    public interface Interface
    {
        Interface A();
    }

    public class Implementor1 : Interface
    {
        Interface f;

        //this method is impure
        public Interface A()
        {
            this.f = new Implementor1();
            return this.f;
        }
    }

    public class Implementor2 : Interface
    {
        Interface f = new Implementor2();

        //this is pure
        public Interface A()
        {
            return f;
        }
        
        //This is impure 
        public static void B(Interface b)
        {
            b.A();
        }

        //this is pure.
        public static void C()
        {
            Interface t = new Implementor2();
            t.A();
        }
    }
}
