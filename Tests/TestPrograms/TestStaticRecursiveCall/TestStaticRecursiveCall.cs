using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestStaticRecursiveCall
{
    public class TestStaticRecursiveCall
    {
        int f = 0;
        public static TestStaticRecursiveCall global = new TestStaticRecursiveCall();

        //Main is not pure as A writes into the second argument passed to it which is a static  object.
        public static void Main(string[] args)
        {
            var r1 = new TestStaticRecursiveCall();
            var r2 = new TestStaticRecursiveCall();

            r1.A(r2, global, args.Length);
        }

        //A calls B and B calls A. Hence, A and B form a SCC. 
        // A - writes to "this" object
        // A-B-A - writes into inst1
        // A-B-A-B-A - writes into inst2
        // Hence, A writes the 'f' field of all its parameters.
        //Note that only the write set changes during each recursion and the points-to graphs remains the same.
        public void A(TestStaticRecursiveCall inst1, TestStaticRecursiveCall inst2, int flag)
        {
            if (flag > 0)
                inst1.B(inst2, flag);
            else
                this.f = 1;
        }

        public void B(TestStaticRecursiveCall inst2, int flag)
        {
            this.A(inst2, this, flag);
        }    
    }
}
