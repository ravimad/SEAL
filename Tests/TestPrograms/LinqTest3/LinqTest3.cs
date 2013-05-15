using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest3
{
    public class LinqTest
    {
        public LinqTest f;
        public int g;
        public int h;

        public void foo(LinqTest param)
        {            
            var col = from elem in IntegerGenerator(param)                      
                      where !StaticShouldFilter(elem)
                      select elem;
            var result = new HashSet<LinqTest>();
            foreach (var y in col)
            {
                y.h = 1;
                result.Add(y);
            }
        }

        private static bool StaticShouldFilter(LinqTest elem)
        {
            elem.f = new LinqTest();
            elem.g++;
            return (elem.g > 0);
        }

        public void bar(LinqTest param)
        {
            var col = from elem in IntegerGenerator(param)
                      where !InstanceShouldFilter(elem)
                      select elem;
            var result = new HashSet<LinqTest>();
            foreach (var y in col)
            {
                y.h = 2;
                result.Add(y);
            }
        }

        private bool InstanceShouldFilter(LinqTest elem)
        {
            elem.f = new LinqTest();
            elem.g++;
            return (elem.g > 0);
        }

        private IEnumerable<LinqTest> IntegerGenerator(LinqTest r)
        {
            for (int x = 0; x < 100; x++)
            {
                yield return r;
            }
        }
    }
}
