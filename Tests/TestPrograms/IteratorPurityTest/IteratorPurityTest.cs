using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IteratorPurityTest
{
    public class IteratorPurityTest
    {
        int f;

        public void foo(IteratorPurityTest lq)
        {            
            foreach (int y in IntegerGenerator(lq))
                Console.WriteLine(y);
        }

        private IEnumerable<int> IntegerGenerator(IteratorPurityTest r)
        {
            for (int x = 0; x < 100; x++)
            {
                r.f = x;
                yield return x;
            }
        }        
    }
}
