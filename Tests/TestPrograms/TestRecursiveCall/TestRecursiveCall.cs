using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestRecursiveCall
{
    //This tests the handling of recursive function calls
    public class TestRecursiveCall
    {
        int f = 0;
        public static TestRecursiveCall global = new TestRecursiveCall();

        //Main is not pure as A writes into the second argument passed to it which is a static  object.
        public static void Main(string[] args)
        {
            TestRecursiveCall r1 = new TestRecursiveCall();
            TestRecursiveCall r2 = new TestRecursiveCall();
                        
            r1.A(r2,global,args.Length);            
        }

        //A calls B and B calls A. Hence, A and B form a SCC. 
        // A - writes to "this" object
        // A-B-A - writes into inst1
        // A-B-A-B-A - writes into inst2
        // Hence, A writes the 'f' field of all its parameters.
        //Note that only the write set changes during each recursion and the points-to graphs remains the same.
        public virtual void A(TestRecursiveCall inst1, TestRecursiveCall inst2, int flag)
        {
            if (flag > 0)
                inst1.B(inst2,flag);
            else
                this.f = 1;
        }

        public virtual void B(TestRecursiveCall inst2, int flag)
        {
            this.A(inst2,this,flag);
        }    
    }
}
