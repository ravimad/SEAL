using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOutParams
{
    public class TestOutParams
    {
        TestOutParams f = null;
        public int g;

        public void m(TestOutParams param, out TestOutParams o)
        {
            o = new TestOutParams();
            o.f = param;
        }

        public void foo(TestOutParams param)
        {
            TestOutParams local;
            m(param,out local);
            local.f.g = 1;
        }
    }
}
