using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestMscorlibCalls
{
    public class TestMscorlibCalls
    {
        String f = "Hello";
        /// <summary>
        /// Two cases of imprecision 
        /// a) l.Add, paramList.Add will write into a static field "emptyArray"
        /// b) Since paramList and l point to the same abstract object on the field "_item"
        /// (due to lack of heap cloning), elem can be this.f and hence it is possible to write into 
        /// this.f.f (can be pruned using the type of elem),
        /// </summary>
        /// <param name="paramList"></param>
        public void foo(List<String> paramList)
        {
            List<TestMscorlibCalls> l = new List<TestMscorlibCalls>();
            l.Add(this);
            foreach (var elem in l)
            {
                elem.f = "Hi";
            }
        }

        public void bar(List<String> paramSet)
        {
            Dictionary<TestMscorlibCalls, String> d = new Dictionary<TestMscorlibCalls, string>();
            d.Add(this, this.f);
            foreach (var key in d.Keys)
            {
                key.f = "Hi";
            }
        }
    }
}
