using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFixPoint
{
    public class TestFixPoint
    {
        int val = -1;
        TestFixPoint f = null;
        public static void Main(string[] args)
        {
            iterate(new TestFixPoint());
        }

        public static void iterate(TestFixPoint inst)
        {
            TestFixPoint current = inst;
            TestFixPoint prev = null;
            while (current.f != null)
            {
                prev = current;
                prev.val = 0;
                current = prev.f;
            }
        }
    }
}
