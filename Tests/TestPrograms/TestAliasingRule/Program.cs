using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestAliasingRule
{
    //This tests the correctness of the aliasing rule.
    class TestAliasingRule
    {
        TestAliasingRule f = null;

        //This method is pure
        public static void Main(string[] args){
            TestAliasingRule inst1 = new TestAliasingRule();
            inst1.A();
        }

        //The object returned by method B is actually the parameter p3. Hence, write to local.f by
        //A() is actually a write to the "this" object. Hence, A() is impure
        public void A(){
            TestAliasingRule inst2 = new TestAliasingRule();
            var local = B(inst2, inst2, this);
            local.f = new TestAliasingRule();
        }

        //B() is impure as it writes to the parameter p2.
        public TestAliasingRule B(TestAliasingRule p1, TestAliasingRule p2, TestAliasingRule p3){
            p2.f = p3;
            return p1.f;
        }
    }
}
