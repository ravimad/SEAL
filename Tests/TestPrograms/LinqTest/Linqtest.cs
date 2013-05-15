using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest
{
    public class LinqTest
    {
        int f;
        public int foo(LinqTest param)
        {             
            var x = from i in IntegerGenerator(param)                    
                    select i;

            int sum = 0;
            foreach (int y in x)
            {
                sum += y;
            }
            return sum;
        }
        private IEnumerable<int> IntegerGenerator(LinqTest r)
        {            
            for (int x = 0; x < 100; x++)
            {                
                r.f = x;                
                yield return x;
            }
        }
    }    
}
