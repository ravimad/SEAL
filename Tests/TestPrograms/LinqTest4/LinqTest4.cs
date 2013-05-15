using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest4
{
    public class LinqTest
    {
        public LinqTest f;
        public int g;
        public int h;
        public LinqTest p;

        private bool InstanceShouldFilter(LinqTest elem)
        {
            elem.f = new LinqTest();
            elem.g++;
            return (elem.g > 0);
        }

        public void baz(LinqTest param, LinqTest m)
        {             
            System.Func<LinqTest, bool> del = (elem => !m.InstanceShouldFilter(elem));
            var col = from elem in IntegerGenerator(param)
                      where del(elem)
                      select elem;
            var result = new HashSet<LinqTest>();
            foreach (var y in col)
            {
                y.h = 1;                
                result.Add(y);
            }
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
