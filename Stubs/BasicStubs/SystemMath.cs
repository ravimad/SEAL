using System;

namespace Stubs
{
    class SystemMath
    {
        public static double Unary(double value)
        {
            return value;
        }

        public static double Binary(double y, double x)
        {
            return (y + x);
        }

        private static unsafe double SplitFractionDouble(double* value)
        {
            return *value;
        }
    }
}
