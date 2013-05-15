using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDictionary
{
    public class TestDictionary
    {
        TestDictionary f;
        Dictionary<TestDictionary, TestDictionary> dict = new Dictionary<TestDictionary,TestDictionary>();
        public void foo(TestDictionary param)
        {
            //var l = new Dictionary<TestDictionary, TestDictionary>();
            //l.Add(param, param.f);
            //TestDictionary val;
            //l.TryGetValue(param, out val);
            //val.f = new TestDictionary();

            TestDictionary val;            
            dict.TryGetValue(param, out val);
            val.f = new TestDictionary();
        }
    }
}
