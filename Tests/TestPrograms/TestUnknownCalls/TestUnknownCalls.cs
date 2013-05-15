using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestUnknownCalls
{    
    //This has a dependency on the TestCH Class hierarchy
    public class TestUnknownCalls
    {
        public void CallUnknownMethod(bool x, TestCH.Interface ti)
        {
            TestCH.Interface ti3 = ti.A();
            TestCH.Interface ti2 = new TestCH.Implementor1();            
            if (x)
            {
                ti2 = new TestCH.Implementor2();
            }
            ti3.A();
            ti2.A();
        }
    }
}
