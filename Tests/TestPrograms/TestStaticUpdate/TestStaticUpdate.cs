using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/** 
 * This testcase tests if the analysis correctly identifies updates to static fields.
 */
namespace TestStaticUpdate
{
    class TestStaticUpdate
    {
        public static TestStaticUpdate stat_ref = new TestStaticUpdate();
        private Object f = null;

        /**
         * Main is also impure as it calls B through A
         */
        static void Main(string[] args)
        {
            (new TestStaticUpdate()).A();
        }

        /**
         * This method is impure as it writes to a static variable through method B 
         */
        public void A()
        {
            B(stat_ref);
        }        
        /**
         * This method is impure as it writes to a static variable.        
         */
        private void B(TestStaticUpdate p)
        {
            p.f = new TestStaticUpdate();
        }
    }
}
