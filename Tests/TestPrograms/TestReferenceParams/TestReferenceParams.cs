using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestReferenceParams
{
    public class TestReferenceParams
    {
        public TestReferenceParams f;
        public int g;
        virtual public void mutatef()
        {
            f.g = 10;
        }

        public void bar(ref TestReferenceParams ins)
        {
            ins.mutatef();
        }

        public void foo(TestReferenceParams inp)
        {
            TestReferenceParams ins = new TestReferenceParams();
            ins.f = inp;
            bar(ref ins);
        }

        public struct TStruct
        {
            public TestReferenceParams f;          

            public void mutateCaller(ref TStruct s)
            {
                s.f.mutatef();
            }

            public void StructFun(TestReferenceParams inp)
            {
                TStruct ins = new TStruct();
                ins.f = inp;
                mutateCaller(ref ins); 
            }
        }
       
    }
}
