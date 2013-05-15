using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestVariableParameters
{
    public class TestVariableParameters
    {
        TestVariableParameters f = null;

        public static void UseParams(TestVariableParameters p1, TestVariableParameters p2, 
            params TestVariableParameters[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (i % 2 == 0)
                    list[i].f = p1.f;
                else
                    list[i].f = p2.f;
            }
        }

        public static void UseParams2(TestVariableParameters l1, TestVariableParameters l2)
        {            
            UseParams(l1,l2,l1,l2);
        }

        public static void Main()
        {
            TestVariableParameters l1 = new TestVariableParameters();
            TestVariableParameters l2 = new TestVariableParameters();                        
            UseParams2(l1,l2);            
        }
    }
}
