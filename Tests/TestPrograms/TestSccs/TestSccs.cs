using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSccs
{
    public class TestSccs
    {
        public void m1(int a, int b)
        {
           m2(a);
           m3(b);           
        }

        public void m2(int c)
        {
            m5(c);
        }

        public void m5(int c)
        {
            m1(0,c);
            m4(c, 0);
        }

        public void m3(int d)
        {
            m2(d);
        }

        public int m4(int a, int b)
        {
            return a + b;
        }
    }
}
