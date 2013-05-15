using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestVirtualMethodCall
{
    class BaseClass
    {
        public BaseClass f;

        //this is pure
        protected virtual BaseClass A(BaseClass param)
        {
            return param.f;
        }

        public void First(String[] args, BaseClass param)
        {
            BaseClass b = new BaseClass();
            if (args.Length > 0)
                b = new DerivedClass1();
            else
                b = new DerivedClass2();
            B(b,param);            
        }        

        //the call to A() can resolve to either DerivedClass1::A() or BaseClass::A(). The first method
        //is impure and the second is pure.
        public static void B(BaseClass b, BaseClass param)
        {
            b.A(param);            
        }
    }

    class DerivedClass1 : BaseClass
    {
        //this method is impure
        protected override BaseClass A(BaseClass param)
        {
            param.f = new DerivedClass1();
            return param.f;
        }
    }

    class DerivedClass2 : BaseClass
    {        
    }
}
