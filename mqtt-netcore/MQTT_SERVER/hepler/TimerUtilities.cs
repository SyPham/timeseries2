using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.hepler
{
   public class TimerUtilities
    {
        public int PERIOD = 0;
        public TimerUtilities()
        {
            this.PERIOD = this.ToRandomNumberInArray(new int [] { });
        }

        /// <summary>
        /// array = [] or new array
        /// </summary>
        /// <param name="array"></param>
        /// <returns>Int in the array.</returns>
        public int ToRandomNumberInArray(int[] array)
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
