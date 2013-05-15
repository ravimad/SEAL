using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestLoadNodeMerge
{
    class TestLoadNodeMerge
    {
        private TestLoadNodeMerge f = null;
        
        void First(TestLoadNodeMerge p)
        {
            TestLoadNodeMerge inst1 = new TestLoadNodeMerge();
            TestLoadNodeMerge inst2 = new TestLoadNodeMerge();
            inst2.f = p;
            inst1.A(inst2);            
        }

        /**
         * The object written by B is also a prestate object for A. Hence A is also impure
         */
        public void A(TestLoadNodeMerge inst)
        {            
            inst.B();
        }

        /**
         * B writes into a prestate object(this.f.f). Hence B is impure.
         */
        public virtual void B()
        {
            this.f.f = new TestLoadNodeMerge();
        }        
    }
}
