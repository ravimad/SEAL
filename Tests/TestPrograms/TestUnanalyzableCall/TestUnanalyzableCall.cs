using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestKnownMethodCall
{
    //Note: the return value of an unanalyzable call will point to global load vertex.
    //The arguments will be marked as escaping via parameters (hence can be captured in the callee)
    class TestUnanalyzableCall
    {        
        TestUnanalyzableCall f = null;

        //The return value (x) of A() is pointing to global load vertex. Hence,
        //write to the field 'f' of 'x' will make Main impure.
        public void foo(TestUnanalyzableCall param)
        {
            var l = new TestUnanalyzableCall();
            l.f = param;
            var x = A(l);
            x.f.f = new TestUnanalyzableCall();
        }

        //this has an unanalyzable method call obj.MemberwiseClone()
        //the return value of obj.MemberwiseClone() is considered as pointing to the global load vertex        
        public static TestUnanalyzableCall A(TestUnanalyzableCall obj)
        {
            //make this an unanalyzable call's return value
            return null;
        }
    }
}
