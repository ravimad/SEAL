using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//This tests the new object summary handler
namespace TestNewObjSummary
{
    class F {
        public int i;

        //pure 
        public F()
        {
            i = 0;
        }        
    };

    class A
    {
        public F f;
        private int x;
        //Pure
        public A(int x)
        {
            this.x = x;
        }
    }

    class Main
    {
        //impure
        void main(F p)
        {
            A first = new A(5);
            first.f = p;
            A returned = foo(first);
            returned.f.i = 20;
        }

        //pure
        A foo(A param)
        {
            A newObj = new A(6);
            newObj.f = param.f;
            return newObj;
        }
    }
}
