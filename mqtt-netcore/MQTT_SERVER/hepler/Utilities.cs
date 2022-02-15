using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MQTT_SERVER.hepler
{
   public static class Utilities
    {
    public static double ToRandomDouble(int range)
        {
            Random r = new Random();
            double rDouble = r.NextDouble() * range; //for doubles
            return rDouble;
        }
        public static int ToRandomInt(int minValue = 0, int maxValue = 100)
        {
            Random r = new Random();
            int rInt = r.Next(minValue, maxValue); //for ints
            return rInt;
        }
        /// <summary>
        /// array = [] or new array
        /// </summary>
        /// <param name="array"></param>
        /// <returns>Int in the array.</returns>
        public static int ToRandomNumberInArray(int[] array)
        {
            if (array.Length == 0)
            {
                array = new[] { 3000, 5000, 8000, 8000, 9000 };
            }
            Random random = new Random();
            int start2 = random.Next(0, array.Length);
            return array[start2];

        }
    }
}
