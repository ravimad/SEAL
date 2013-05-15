using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGenericMethods
{     
    public class Dummy
    {
        public Dummy f = null;
    }

    public class Doom
    {
        public Doom f = null;
    }   

    interface TestInter<T>         
    {        
        T m(T arg);
    }

    class TestImpl1<T> : TestInter<T>
        where T : Dummy
    {        
        public T m(T arg)
        {           
            arg.f = new Dummy();
            return arg;
        }
    }

    class TestImpl2 : TestInter<Doom>        
    {
        public Doom m(Doom arg)
        {
            arg.f = new Doom();
            return arg.f;
        }
    }  

    public class TestBase<T>
    {
        public virtual T m(T arg)
        {
            return arg;
        }
    }

    public class TestDerived : TestBase<Dummy>
    {
        public override Dummy m(Dummy arg)
        {
            arg.f = new Dummy();
            return arg.f;
        }
    }

    public class mainclass
    {
        public static void foo1(Doom param)
        {
            TestInter<Doom> inter = new TestImpl2();
            inter.m(param);
        }

        public static void foo2(Dummy param)
        {
            TestBase<Dummy> b = new TestDerived();
            b.m(param);
        }
    }
}