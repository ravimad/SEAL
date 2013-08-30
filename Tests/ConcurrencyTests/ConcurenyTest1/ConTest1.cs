/********************************************************
*                                                       *
*     Copyright (C) Microsoft. All rights reserved.     *
*                                                       *
********************************************************/

using System;
using System.Threading;

namespace Test6
{
    public class ConcurrencyTest1
    {
        Outer f;        
        public static void Main(string[] s)
        {
            var a = new ConcurrencyTest1();
            a.f = new Outer();
            var b = new Outer();
            Run(a,b);   
        }        
        public static bool Run(ConcurrencyTest1 a, Outer b)
        {           
            Thread t = new Thread(delegate(object o)
            {
                var c = (ConcurrencyTest1)o;
                c.f = b;
            });
            t.Start(a);
            // put parent thread code here
            a.f.t = new ConcurrencyTest1();
            t.Join();
            return true;
        }        
    }

    public class Outer
    {
        public ConcurrencyTest1 t;
    }
}