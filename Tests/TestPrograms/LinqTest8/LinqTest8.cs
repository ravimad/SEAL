using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest8
{
    public class LinqTest8
    {
        int f = 0;
        public IEnumerable<T> foo<T>(IEnumerable<T> param, System.Func<T,bool> filter)
            where T : LinqTest8
        {            
            var col = from x in param
                      where filter(x)
                      select x;

            foreach (var y in col)
            {
                y.f = 2;
            }
            return col;
        }
    }
}
