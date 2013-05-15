using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestsLibraryCalls
{
    public class TestLibraryCalls
    {
        String f = "Hello";
        public void foo(List<String> paramList)
        {
            List<TestLibraryCalls> l = new List<TestLibraryCalls>();
            l.Add(this);
            foreach (var elem in l)
            {
                elem.f = "Hi";
                paramList.Add(elem.f);
            }
        }

        public void bar(HashSet<String> paramSet)
        {
            Dictionary<TestLibraryCalls, String> d = new Dictionary<TestLibraryCalls, string>();
            d.Add(this, this.f);
            foreach (var key in d.Keys)
            {
                key.f = "Hi";
                paramSet.Add(key.f);
            }
        }

        public void baz(Queue<String> paramQ)
        {
            Queue<TestLibraryCalls> q = new Queue<TestLibraryCalls>();
            q.Enqueue(this);
            while (q.Any())
            {
                var elem = q.Dequeue();
                elem.f = "Hi";
                paramQ.Enqueue(elem.f);
            }
        }

        public void boomerang(Stack<String> paramS)
        {
            Stack<TestLibraryCalls> s = new Stack<TestLibraryCalls>();
            s.Push(this);
            while (s.Any())
            {
                var elem = s.Pop();
                elem.f = "Hi";
                paramS.Push(elem.f);
            }
        }

        public void bogus(List<String> paramSet)
        {
            HashSet<TestLibraryCalls> hs = new HashSet<TestLibraryCalls>();
            hs.Add(this);
            foreach (var elem in hs)
            {
                elem.f = "Hi";
                paramSet.Add(elem.f);
            }
        }
    }
}
