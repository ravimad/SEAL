using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNestedLinq
{
    public class NestedLinq
    {
        public int f;
        public NestedLinq foo(NestedLinq param)
        {
            var innercol = CollectionGenerator(param);
            var outercol = CollectionGenerator(innercol);

            var col = from outerelem in outercol
                      from innerelem in outerelem
                      select innerelem;

            foreach (NestedLinq y in col)
            {
                y.f = 2;
            }
            return param;
        }

        private IEnumerable<NestedLinq> CollectionGenerator(NestedLinq r)
        {
            for (int x = 0; x < 100; x++)
            {
                yield return r;
            }
        }

        private IEnumerable<IEnumerable<NestedLinq>> CollectionGenerator(IEnumerable<NestedLinq> r)
        {
            for (int x = 0; x < 100; x++)
            {
                yield return r;
            }
        }
    }
}
