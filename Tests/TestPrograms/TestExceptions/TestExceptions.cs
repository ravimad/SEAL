using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestExceptions
{
    public class TestExceptions
    {
        public int f = 0;

        public void foo(int a)
        {
            try
            {
                bar();
            }
            catch (MyException e)
            {
                e.te.f = -1;
                throw e;
            }
        }

        public void bar()
        {
            for (int i = 0; i < 100; i++)
            {
                if (i % 99 == 1)
                    throw new MyException(this);
            }
        }
    }

    public class MyException : Exception
    {
        public TestExceptions te;
        public String MyMessage;
        public MyException innerException;

        public MyException(TestExceptions testExcep)
        {
            te = testExcep;
        }
    }
}
