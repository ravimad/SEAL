using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest2
{
    public class LinqTest
    {
        int f;
        public void foo(LinqTest param)
        {             
            var col = from i in IntegerGenerator(param)                    
                      select i;

            foreach (LinqTest y in col)
            {
                y.f = 2;
            }
        }

        private IEnumerable<LinqTest> IntegerGenerator(LinqTest r)
        {            
            for (int x = 0; x < 100; x++)
            {                                                
                yield return r;
            }
        }

        public void bar(LinqTest param1)
        {            
            var list = new List<LinqTest>();
            list.Add(param1);

            var col = from elem in list
                      select elem;
            foreach (LinqTest y in col)
            {
                y.f = 2;
            }
        }

        public void baz(LinqTest param2)
        {
            var set = new HashSet<LinqTest>();
            set.Add(param2);

            var col = from elem in set
                      select elem;
            foreach (LinqTest y in col)
            {
                y.f = 2;
            }
        }
    }    
}
