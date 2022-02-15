using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.hepler
{
   public static class MyConsole
    {

       public static void Error(string value)
        {
            // Write an entire line to the console with the string.
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(value);
            // Reset the color.
            Console.ResetColor();
        }
        public static void Info(string value)
        {
            // Write an entire line to the console with the string.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value);
            // Reset the color.
            Console.ResetColor();
        }
        public static void Data(string value)
        {
            // Write an entire line to the console with the string.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(value);
            // Reset the color.
            Console.ResetColor();
        }
        public static void Warning(string value)
        {
            // Write an entire line to the console with the string.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(value);
            // Reset the color.
            Console.ResetColor();
        }
        public static void Connected(string value)
        {
            // Write an entire line to the console with the string.
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(value);
            // Reset the color.
            Console.ResetColor();
        }
        public static void Disconnected(string value)
        {
            // Write an entire line to the console with the string.
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(value);
            // Reset the color.
            Console.ResetColor();
        }
    }
}
