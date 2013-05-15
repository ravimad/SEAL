using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSystemCoreCalls
{
    public class TestSystemCoreCalls
    {
        String f = "Hello";
        public void foo(HashSet<String> paramSet)
        {
            HashSet<TestSystemCoreCalls> hs = new HashSet<TestSystemCoreCalls>();
            hs.Add(this);
            foreach (var elem in hs)
            {
                elem.f = "Hi";                
            }
        }
    }
}
