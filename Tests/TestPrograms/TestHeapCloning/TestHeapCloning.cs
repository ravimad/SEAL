using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHeapCloning
{
    public class TestHeapCloning
    {
        TestHeapCloning f;
        int h;
        public void foo(TestHeapCloning param)
        {
            TestHeapCloning local1 = bar();
            TestHeapCloning local2 = bar();
            local1.f = param;
            local2.f = new TestHeapCloning();
            local2.f.h = 1;
        }

        public TestHeapCloning bar()
        {
            TestHeapCloning r = new TestHeapCloning();
            return r;
        }
    }
}
