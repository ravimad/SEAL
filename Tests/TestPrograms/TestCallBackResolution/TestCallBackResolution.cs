using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCallBackResolution
{
    class BaseClass
    {
        public BaseClass f;

        //this is pure
        protected virtual BaseClass A(BaseClass arg)
        {
            return arg.f;
        }

        public static void C(BaseClass arg)
        {
            BaseClass b = new BaseClass();

            if (arg.f != null)
                b = new DerivedClass1();
            else
                b = new DerivedClass2();
            B(b,arg);            
        }        

        //the call to A() can resolve to either DerivedClass1::A() or BaseClass::A(). The first method
        //is impure and the second is pure. 
        public static void B(BaseClass b, BaseClass arg)
        {
            b.A(arg);
        }        
    }

    class DerivedClass1 : BaseClass
    {
        //this method is impure
        protected override BaseClass A(BaseClass arg)
        {
            arg.f = new DerivedClass1();
            return arg.f;
        }
    }

    class DerivedClass2 : BaseClass
    {        
    }
}
