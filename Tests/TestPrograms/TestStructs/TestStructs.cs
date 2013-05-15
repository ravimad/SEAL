using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestStructs
{
    public class TestStructs
    {
        public struct tstruct
        {
            public int f;
            public int g;
            public tstruct(int i,int j)
            {
                f = i;
                g = j;
            }
        }
        tstruct st;
        //TestStructs instf;
        public void foo(TestStructs inst)
        {            
            var i = inst.st.f;
            i++;
            inst.st.f = i;
        }

        public void bar(TestStructs inst)
        {           
            inst.st = new tstruct(0, 1);
        }

        public tstruct baz()
        {
            tstruct st;
            st.f = 1;
            st.g = 0;
            return st;
        }
    }
}
