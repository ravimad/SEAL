using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOrderBy
{
    public class TestOrderBy
    {
        public bool live = true;
        public int f;

        public void foo(TestOrderBy param)
        {
            var list = new List<TestOrderBy> { param };
            var sortedcol = bar(list);
            foreach (var elem in sortedcol)
            {
                elem.f = 1;                
            }            
        }       

        public IEnumerable<TestOrderBy> bar(IEnumerable<TestOrderBy> attributes)
        {
            return attributes.Where(a => a.live).OrderBy((TestOrderBy a) => (a.f));                                                                                                                            
        }
    }
}
