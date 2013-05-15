using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestRecursiveCall2
{
    //This tests the handling of recursive function calls
    public class TestRecursiveCall2
    {
        int f;
        public TestRecursiveCall2 g;
        public static TestRecursiveCall2 global = new TestRecursiveCall2();

        //Main is not pure as A writes into the second argument passed to it which is a static  object.
        public void First(TestRecursiveCall2 p1, TestRecursiveCall2 p2, int flag)
        {
            TestRecursiveCall2 r1 = new TestRecursiveCall2();
            TestRecursiveCall2 r2 = new TestRecursiveCall2();

            r1.g = p1;
            r2.g = p2;
            r1.A(r2,global,flag);            
        }

        //A calls B and B calls A. Hence, A and B form a SCC. 
        // A - writes to "this" object
        // A-B-A - writes into inst1
        // A-B-A-B-A - writes into inst2
        // Hence, A writes the 'f' field of all its parameters.
        //Note that only the write set changes during each recursion and the points-to graphs remains the same.
        public virtual void A(TestRecursiveCall2 inst1, TestRecursiveCall2 inst2, int flag)
        {
            if (flag > 0)
                inst1.B(inst2,flag);
            else
                this.g.f = 1;
        }

        public virtual void B(TestRecursiveCall2 inst2, int flag)
        {
            this.A(inst2,this,flag);
        }    
    }
}
