using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestEscapingNodes
{
    public class Test1
    {
        public int h;
    }

    public class Test2
    {
        public Test1 g = null;
    }

    public class TestEscapingNodes
    {
        Test2 f = null;        
        public void foo(int x, Test1 p)
        {            
            TestEscapingNodes t = new TestEscapingNodes();            
            bar(p,t,t);
        }

        public void bar(Test1 s, TestEscapingNodes q, TestEscapingNodes r)
        {
            Test2 t2 = new Test2();
            q.f = t2;
            r.f.g = s;
            t2.g.h = 1;            
        }
    }
}
