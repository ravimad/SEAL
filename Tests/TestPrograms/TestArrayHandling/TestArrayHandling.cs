using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestArrayHandling
{
    public class TestArrayHandling
    {
        public int[] a = new int[10];

        public static void Main(string[] args)
        {
            (new TestArrayHandling()).foo(args.Length);
        }

        public void foo(int i)
        {
            a[i] = 0;
        }
    }
}
