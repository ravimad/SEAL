using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestJoin
{
    public class TestJoin
    {
        public int f = 0;
        public void foo(TestJoin param1, TestJoin param2)
        {
            var list1 = new List<TestJoin> { param1 };
            var list2 = new List<TestJoin> { param2 };

            var joinlist = list1.Join<TestJoin,TestJoin,TestJoin,Object[]>(
                list2,(TestJoin j) => j, (TestJoin i) => i, (TestJoin i, TestJoin j) =>  (new Object[] {i, j}));
            foreach (var elem in joinlist)
            {
                (elem[0] as TestJoin).f = 1;
                (elem[1] as TestJoin).f = 1;
            }
        }
    }
}
