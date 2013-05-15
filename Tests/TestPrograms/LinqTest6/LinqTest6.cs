using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqTest6
{
    public class LinqTest
    {
        public int f;
         public void foo(LinqTest lt)
         {
             var col = new List<LinqTest> { lt };
             var y = new List<LinqTest>(RemoveDefaults(col));
             var x = y[0];
             x.f = 1;
         }
        /// <summary>
        /// Removes default values from a list
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="Value">List to cull items from</param>
        /// <param name="EqualityComparer">Equality comparer used (defaults to GenericEqualityComparer)</param>
        /// <returns>An IEnumerable with the default values removed</returns>
        public IEnumerable<T> RemoveDefaults<T>(IEnumerable<T> Value)
        {
            if (Value == null)
                yield break;            
            foreach (T Item in Value.Where(x => !x.Equals(default(T))))
                yield return Item;
        }
    }
}