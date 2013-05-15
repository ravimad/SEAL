using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestVirtualDelegate
{
    public class TestVirtualDelegate
    {
        TestVirtualDelegate f = null;
        public TestVirtualDelegate g = null;
        delegate int myDelegate(TestVirtualDelegate testDel);

        public virtual int baz(TestVirtualDelegate testDel)
        {
            testDel.f = new TestVirtualDelegate();
            return 0;
        }

        public void start()
        {
            var testDel = new TestVirtualDelegate2();
            foo(testDel);
        }

        public void foo(TestVirtualDelegate testDel)
        {
            myDelegate del = testDel.baz;
            del(this);
        }
    }

    public class TestVirtualDelegate2 : TestVirtualDelegate
    {
        public override int baz(TestVirtualDelegate testDel)
        {
            testDel.g = new TestVirtualDelegate();
            return 0;
        }
    }
}
