using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestKnownMethodCalls
{
    public class TestKnownMethodCalls
    {
        TestKnownMethodCalls f = null;

        //The return value (x) of A() is pointing to global load vertex. Hence,
        //write to the field 'f' of 'x' will make Main impure.
        public void foo(TestKnownMethodCalls param)
        {
            var l = new TestKnownMethodCalls();
            l.f = param;
            var x = A(l);           
            x.f.f = new TestKnownMethodCalls();
        }

        //this has an unanalyzable method call obj.MemberwiseClone()
        //the return value of obj.MemberwiseClone() is considered as pointing to the global load vertex        
        public static TestKnownMethodCalls A(TestKnownMethodCalls obj)
        {
            return obj.MemberwiseClone() as TestKnownMethodCalls;
        }
    }
}
