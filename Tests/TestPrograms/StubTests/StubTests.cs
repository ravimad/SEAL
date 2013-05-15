using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StubTests
{
    public class StubTests
    {
        public int f;
        public void foo(StubTests inst1)
        {
            var local = Array.CreateInstance(Type.GetType("StubTests",true),10);
            local.SetValue(inst1, 2);
            var elem = (StubTests)local.GetValue(2);
            elem.f = 1;
        }

        public void bar(Array array)
        {
            Array.Clear(array,0,100);
        }

        public void baz(Array src)
        {
            var dest = Array.CreateInstance(Type.GetType("StubTests", true), 10);
            Array.Copy(src, dest, 10);
            var val = (StubTests)dest.GetValue(9);
            val.f = 1;
        }

        public void m1(StubTests inst)
        {
            var local = new StubTests();
            System.Threading.Interlocked.CompareExchange(ref local, inst, local);
            local.f = 1;
        }

        public void m2(StubTests inst)
        {
            var local = new StubTests();
            System.Threading.Interlocked.Exchange(ref local, inst);
            local.f = 1;
        }
    }
}
