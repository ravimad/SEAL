using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSystemLibCalls
{
    public class TestSystemLibCalls
    {
        String f = "Hello";

        public void foo(Queue<String> paramQ)
        {
            Queue<TestSystemLibCalls> q = new Queue<TestSystemLibCalls>();
            q.Enqueue(this);
            while (q.Any())
            {
                var elem = q.Dequeue();
                elem.f = "Hi";                
            }
        }

        public void bar(Stack<String> paramS)
        {
            Stack<TestSystemLibCalls> s = new Stack<TestSystemLibCalls>();
            s.Push(this);
            while (s.Any())
            {
                var elem = s.Pop();
                elem.f = "Hi";                
            }
        }
    }
}
