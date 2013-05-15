using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGenerics
{
    public class I
    {
        public I g = null;
    }    

    public class A<T>
    {
        T f;
        public void m1(A<T> arg1, T arg2)
        {
            arg1.f = arg2;
        }

        public bool m2(A<T> arg1, T arg2)
        {
            var b1 = arg1.f.Equals(arg2);
            var b2 = arg2.Equals(arg1.f);            
            return b1 & b2;
        }

        public void m3(A<T> arg1, T arg2)
        {
            var i = arg1.f as I;
            i.Equals(arg2);
        }

        public bool m4<U>(U arg1)
            where U : I
        {
            return arg1.g.Equals(new I());
        }
    }
}
