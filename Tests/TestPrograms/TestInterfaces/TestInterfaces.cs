﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestInterfaces
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

        public static void Main(string[] args)
        {
            Interface b;

            if (args.Length > 0)
                b = new Implementor1();
            else
                b = new Implementor2();
            B(b);
        }

        //the call to A() can resolve to either DerivedClass1::A() or BaseClass::A(). The first method
        //is impure and the second is pure. The analysis should decipher that B() is impure.
        public static void B(Interface b)
        {
            b.A();
        }
    }
}
