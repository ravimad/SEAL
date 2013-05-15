using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNodeMerging
{    
    public class TestNodeMerging
    {
        TestNodeMerging f;

        public void foo(TestNodeMerging a, TestNodeMerging b, TestNodeMerging c)
        {
            var l1 = new TestNodeMerging();
            l1.f = c;

            var l2 = new TestNodeMerging();
            var l3 = new TestNodeMerging();
            l3.f = new TestNodeMerging();
            l2.f = l3;            
            a.f = l1;
            a.f = l2;
            b.f = l2;
        }

        public void bar(TestNodeMerging c)
        {
            var l3 = new TestNodeMerging();
            var l4 = new TestNodeMerging();
            foo(l3,l4,c);
            l4.f.f.f.f = new TestNodeMerging();
        }        
    }
}
