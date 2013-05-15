using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestLibIEnumerables
{
    public class TestLibIEnumerables
    {
        String f = "Hello";
        public void foo(List<String> outSet)
        {
            List<TestLibIEnumerables> l = new List<TestLibIEnumerables>();
            l.Add(this);
            bar(l, outSet);
        }

        public void bar(IEnumerable<TestLibIEnumerables> inSet, List<String> outSet)
        {
            foreach (var elem in inSet)
            {
                elem.f = "Hi";
                outSet.Add(elem.f);
            }
        }
    }
}
