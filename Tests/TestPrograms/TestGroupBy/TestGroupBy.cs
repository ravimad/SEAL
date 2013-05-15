using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGroupBy
{
    public class TestGroupBy
    {
        public TestGroupBy f = null;
        public int g = 0;

        public void foo(TestGroupBy param)
        {
            var list = new List<TestGroupBy> { param };
            var map = from elem in list
                      group elem.f by elem.f.g;
            foreach (var group in map)
            {
                foreach (var mem in group)
                    mem.g = 1;
            }
        }
    }
}
