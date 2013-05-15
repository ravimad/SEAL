using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSkippedCallsMerging
{
    public class TestSkippedCallsMerging
    {
        private TestSkippedCallsMerging f;
        public void foo(TestSkippedCallsMerging param1, TestSkippedCallsMerging param2)
        {
            var arg1 = param1.f;
            var arg2 = param2.f;

            if(arg1.Equals(arg2) && bar(param1, param2))
            {
                arg1.f = arg2;
            }
        }

        public bool bar(TestSkippedCallsMerging param1, TestSkippedCallsMerging param2)
        {
            return param1.f.Equals(param2.f);
        }        
    }
}
