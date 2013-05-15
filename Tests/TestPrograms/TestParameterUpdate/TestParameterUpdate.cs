using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test2
{
    class Test2 
    {
        private int f = 0;

        //this is impure
        public void C(Test2 inst3)
        {
            Test2 inst1 = new Test2();
            Test2 inst2 = new Test2();
            inst1.B();
            inst1.A(inst2);
            var temp = inst3.foo();
            temp.f = 3;
        }

        //this is impure
        public void A(Test2 inst)
        {
            inst.f = 1;
        }

        //this is impure
        public void B()
        {
            this.f = 2;
        }

        //this is pure, it only uses the parameter
        public Test2 foo()
        {
            return this;
        }
    }
}
