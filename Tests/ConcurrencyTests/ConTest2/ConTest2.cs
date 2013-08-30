using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.Threading;

namespace Test6
{
    public class ConcurrencyTest2
    {
        public Outer f;
        public static void Main(string[] s)
        {
            var a = new ConcurrencyTest2();
            a.f = new Outer();
            var b = new Outer();
            Run(a, b);
        }
        public static bool Run(ConcurrencyTest2 a, Outer b)
        {
            Thread t = new Thread(delegate(object o)
            {
                var c = (ConcurrencyTest2)o;
                //this is a call-back
                b.foo(c);
            });
            t.Start(a);
            var x = a.f;            
            a.f.g = new ConcurrencyTest2();
            t.Join();
            return true;
        }
    }

    public class Outer
    {
        public ConcurrencyTest2 g;

        public virtual bool foo(ConcurrencyTest2 c)
        {
            c.f = this;
            return true;
        }
    }
}