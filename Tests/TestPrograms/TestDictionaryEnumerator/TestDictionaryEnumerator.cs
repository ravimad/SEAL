using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDictionaryEnumerator
{
    public class TestDictionaryEnumerator
    {
        TestDictionaryEnumerator f = null;
        String g = "Hello";

        public void foo(TestDictionaryEnumerator param)
        {
            Dictionary<TestDictionaryEnumerator, TestDictionaryEnumerator> l
                = new Dictionary<TestDictionaryEnumerator, TestDictionaryEnumerator>();            
            l.Add(param, param.f);
            bar(l.Values);
        }

        public void bar(IEnumerable<TestDictionaryEnumerator> inSet)
        {
            foreach (var elem in inSet)
            {
                elem.g = "Hi";                
            }
        }
    }
}
