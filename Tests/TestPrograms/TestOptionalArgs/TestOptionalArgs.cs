using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOptionalArgs
{
    class TestOptionalArgs
    {
        int f;
        public void foo(TestOptionalArgs req, int optional = 0)
        {
            req.f = optional;
        }

        static void Main(string[] args)
        {
            (new TestOptionalArgs()).foo(new TestOptionalArgs());
        }
    }
}
