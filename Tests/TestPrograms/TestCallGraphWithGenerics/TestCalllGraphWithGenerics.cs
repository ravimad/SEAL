using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCallGraphWithGenerics
{
    public class TestCalllGraphWithGenerics
    {
        class BaseClass
        {
            public BaseClass f;

            protected virtual BaseClass A<T>(T param)
            {                
                return f;
            }

            //the call to A() can resolve to DerivedClass2::A() or DerivedClass1::A().
            //Hence, this is impure.
            public static void B(BaseClass b)
            {
                char a = (char)48;
                b.A<char>(a);
            }
        }

        class DerivedClass1 : BaseClass
        {
            //this method is impure
            protected override BaseClass A<U>(U param)
            {
                this.f = new DerivedClass1();
                return this.f;
            }
        }

        class DerivedClass2 : BaseClass
        {                        
        }
    }
}
