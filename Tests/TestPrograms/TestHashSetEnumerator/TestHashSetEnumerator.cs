using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHashSetEnumerator
{
    public class TestHashSetEnumerator
    {
        String f = "Hello";
        public void foo(HashSet<String> outSet)
        {
            HashSet<TestHashSetEnumerator> l = new HashSet<TestHashSetEnumerator>();
            l.Add(this);
            bar(l);
        }

        public void bar(IEnumerable<TestHashSetEnumerator> inSet)
        {
            foreach (var elem in inSet)
            {
                elem.f = "Hi";                
            }
        }
    }
}
