using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenericCalls
{
    public class TestGenericCalls
    {
        public TestGenericCalls f;
        public int g;        

        public override int GetHashCode()
        {
            f.g = 10;
            return base.GetHashCode();
        }          

        public void bar<T>(T arg1)
        {
            arg1.GetHashCode();
        }

        public void foo(TestGenericCalls inp)
        {
            TestGenericCalls ins = new TestGenericCalls();
            ins.f = inp;
            bar(ins);
        }

        public struct TStruct
        {
            public TestGenericCalls f;
            int g;
            public override int GetHashCode()
            {                
                return f.GetHashCode();
            }
        }
        public void StructFun(TestGenericCalls inp)
        {
            TStruct ins = new TStruct();
            ins.f = inp;
            bar(ins); // here, the struct is copied, which means the write happen to local object
        }
    }
}
