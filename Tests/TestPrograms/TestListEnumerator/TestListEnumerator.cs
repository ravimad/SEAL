using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestListEnumerator
{
    public class TestListEnumerator
    {
        String f = "Hello";
        public void foo(List<String> outSet)
        {
            List<TestListEnumerator> l = new List<TestListEnumerator>();
            l.Add(this);
            bar(l, outSet);         
        }

        public void bar(IEnumerable<TestListEnumerator> inSet, List<String> outSet)
        {
            foreach (var elem in inSet)
            {
                elem.f = "Hi";
                outSet.Add(elem.f);
            }
        }
    }
}
