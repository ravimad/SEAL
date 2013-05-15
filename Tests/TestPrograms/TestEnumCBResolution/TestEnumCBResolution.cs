using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestEnumCBResolution
{
    public class TestEnumCBResolution
    {
        public String f;
        public int intval;

        class SingletonCol<T> : IEnumerable<T>
        {
            T content;

            public void Add(T e)
            {
                content = e;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
 	            yield return content;
                yield break;
            }

            System.Collections.IEnumerator  System.Collections.IEnumerable.GetEnumerator()
            {
                return null;
            }
        }
        
        public void foo(TestEnumCBResolution param)
        {
            var col = new SingletonCol<TestEnumCBResolution>();
            col.Add(param);
            bar(col);
        }

        public void bar(IEnumerable<TestEnumCBResolution> col)
        {
            var iter = col.GetEnumerator();
            while (iter.MoveNext())
            {
                var elem = iter.Current;
                elem.f = "Hi";
                elem.intval = 2;
            }
        }
    }
}
