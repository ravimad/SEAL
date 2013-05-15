using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest7
{
    public class LinqTest
    {
        public int f = 0;
        public void foo(LinqTest param)
        {            
            var col1 = new List<LinqTest> { param };
            var y = bar(col1);

            var col2 = new List<LinqTest> { new LinqTest() };
            foreach (var z in bar(col2))
            {
                z.f = 1;
            }
        }

        public IEnumerable<LinqTest> bar(IEnumerable<LinqTest> col)
        {
            return from x in col
                   select x;
        }
    }
}
