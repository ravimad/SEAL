using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestInstanceDelegates
{
    class TestInstanceDelegates
    {
        TestInstanceDelegates f = null;        

        void foo(System.Func<TestInstanceDelegates, int> del)
        {
            del(this);
        }

        public void start(TestInstanceDelegates testDel1, TestInstanceDelegates testDel2)
        {            
            testDel1.foo((TestInstanceDelegates param) =>
            {
                testDel2.f = new TestInstanceDelegates();
                param.f = testDel2.f;
                return 0;
            });
        }
    }
}
