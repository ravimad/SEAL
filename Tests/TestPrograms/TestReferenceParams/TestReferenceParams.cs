using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void barGen<T>(ref T ins) where T : TestReferenceParams
        {
            ins.mutatef();
        }

        public void foo(TestReferenceParams inp)
        {
            TestReferenceParams ins = new TestReferenceParams();
            ins.f = inp;
            bar(ref ins);
        }

        public void foo2(TestReferenceParams inp)
        {
            TestReferenceParams ins = new TestReferenceParams();
            ins.f = inp;
            bar(ref ins);
        }

        // The following is a buggy case of handling structs in Seal which needs to be fixed
        //public struct TStruct
        //{
        //    public TestReferenceParams f;          

        //    public void mutateCaller(ref TStruct s)
        //    {
        //        s.f.mutatef();
        //    }

        //    public void StructFun(TestReferenceParams inp)
        //    {
        //        TStruct ins = new TStruct();
        //        ins.f = inp;
        //        mutateCaller(ref ins); 
        //    }
        //}
    }
}
