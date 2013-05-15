using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCallGraph
{
    class BaseClass
    {
        public BaseClass f;

        protected virtual BaseClass A() 
        {
            return f;
        }

        //the call to A() can resolve to DerivedClass2::A() only. Which is pure and hence B() is pure.
        public static void B(DerivedClass2 d2)
        {
            d2.A();
        }        
    }

    class DerivedClass1 : BaseClass
    {
        //this method is impure
        protected override BaseClass A()
        {
            this.f = new DerivedClass1();
            return this.f;
        }
    }

    class DerivedClass2 : BaseClass
    {
        protected override BaseClass A()
        {
            return new DerivedClass2();
        }
    }
}
