using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stubs
{
    public class SystemArrayStub
    {
        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable)
        {
            var src = (Object[])sourceArray;
            var dest = (Object[])destinationArray;

            for (int i = sourceIndex; i <= destinationIndex; i++)
            {
                dest[i] = src[i];
            }
        }

        public static void Clear(Array array, int index, int length)
        {
            var arr = (object[])array;
            for (int i = index; length > 0; length--)
            {
                arr[i] = 0;
            }
        }

        public static int GetLength(System.Array thisRef, int dimension)
        {
            return 0;
        }

        public static int GetLowerBound(System.Array thisRef, int dimension)
        {
            return 0;
        }

        public static int GetUpperBound(System.Array thisRef, int dimension)
        {
            return 0;
        }

        public static void Initialize(System.Array thisRef)
        {
        }
       
        public static object GetValue(System.Array thisRef, params int[] indices)
        {
            var arrayObj = (Object[])thisRef;
            return arrayObj[indices[0]];
        }

        public static object GetValue(System.Array thisRef, int index)
        {
            var arrayObj = (Object[])thisRef;
            return arrayObj[index];
        }

        public static object GetValue(System.Array thisRef, int index1, int index2)
        {
            var arrayObj = (Object[][])thisRef;
            return arrayObj[index1][index2];
        }

        public static object GetValue(System.Array thisRef, int index1, int index2, int index3)
        {
            var arrayObj = (Object[][][])thisRef;
            return arrayObj[index1][index2][index3];
        }

        public static void SetValue(System.Array thisRef, object value, int index)
        {
            var arrayObj = (Object[])thisRef;
            arrayObj[index] = value;
        }
      
        public static void SetValue(System.Array thisRef, object value, params int[] indices)
        {
            var arrayObj = (Object[])thisRef;
            arrayObj[indices[0]] = value;
        }

        public static void SetValue(System.Array thisRef, object value, int index1, int index2)
        {
            var arrayObj = (Object[][])thisRef;
            arrayObj[index1][index2] = value;
        }

        public static void SetValue(System.Array thisRef, object value, int index1, int index2, int index3)
        {
            var arrayObj = (Object[][][])thisRef;
            arrayObj[index1][index2][index3] = value;
        }

        private static bool TrySZBinarySearch(Array sourceArray, int sourceIndex, int count, object value, out int retVal)
        {
            retVal = 0;
            return true;
        }

        private static bool TrySZIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal)
        {
            retVal = 0;
            return true;
        }

        private static bool TrySZLastIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal)
        {
            retVal = 0;
            return true;
        }

        private static bool TrySZReverse(Array array, int index, int count)
        {
            return true;
        }

        private static bool TrySZSort(Array keys, Array items, int left, int right)
        {
            return true;
        }

        public static int get_Length(System.Array thisRef)
        {
            return 0;
        }

        public static int get_Rank(System.Array thisRef)
        {
            return 0;
        }
    }

    public static class Interlocked
    {       
        public static int Add(ref int location1, int value)
        {
            return location1 + value;
        }

        public static T CompareExchange<T>(ref T location1, T value, T comparand)            
        {
            var temp = location1;
            if(location1.Equals(comparand))
                location1 = value;
            return temp;
        }        

        public static int Decrement(ref int location)
        {
            return location--;
        }

        public static T Exchange<T>(ref T location1, T value) 
        {
            var temp = location1;
            location1 = value;
            return temp;
        }

        internal static int ExchangeAdd<T>(ref int location1, int value)
        {
            var temp = location1;
            location1 = value;
            return temp + 1;
        }

        public static int Increment(ref int location)
        {
            return location++;
        }

        public static int Read(ref int location)
        {
            return location;
        }
    }

    public static class SystemDelegate
    {
        public static Delegate Combine(Delegate d1, Delegate d2)
        {
            //this captures non-determinism
            if (d1 is Action)
                return d1;
            else return d2;
        }

        public static Delegate Combine(Delegate[] darray)
        {
            for (int i = 0; i < darray.Length; i++)
            {
                //this captures non-determinism
                if (i % 2 == 0)
                    return darray[i];                
            }
            return null;
        }
    }

    public class HashHelpers
    {
        public static long GetEntropy()
        {
            return 0;
        }
    }
}
