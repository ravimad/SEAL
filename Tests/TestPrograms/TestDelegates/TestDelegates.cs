using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDelegates
{
    class TestDelegates
    {
        TestDelegates f = null;
        delegate int myDelegate(TestDelegates testDel);

        void foo(System.Func<TestDelegates,int> del)
        {
            del(this);
        }

        public static int baz(TestDelegates testDel)
        {            
            testDel.f = new TestDelegates();            
            return 0;
        }

        void bar(myDelegate del)
        {
            del(this);
        }

        public void start(TestDelegates testDel1, TestDelegates testDel2)
        {
            testDel1.foo((TestDelegates param) =>
            {
                param.f = new TestDelegates();
                return 0;
            });            

            myDelegate del = baz;
            testDel2.bar(del);
        }
    }
}
