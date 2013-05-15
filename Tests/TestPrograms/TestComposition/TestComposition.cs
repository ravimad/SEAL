using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestComposition
{
    public class TestComposition
    {
        TestComposition f;

        public void foo(TestComposition lq)
        {            
            foreach (int y in IntegerGenerator(lq,lq.f))
                Console.WriteLine(y);
        }

        private IEnumerable<int> IntegerGenerator(TestComposition r,TestComposition param)
        {
            for (int x = 0; x < 100; x++)
            {
                r.f = param;
                yield return x;
            }
        }        
    }
}
