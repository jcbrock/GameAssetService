using System;

namespace GameServiceTests.TestingExtensions
{
    public static class IntExtensions
    {
        static Random _rnd = new Random();

        /// <summary>
        /// Returns a random number greater than or equal to 0 and less than the integer
        /// </summary>
        /// <param name="maxValue">The non-inclusive maximum integer value for the random number</param>
        /// <returns>A random integer</returns>
        public static int GetRandom(this int maxValue)
        {
            return maxValue.GetRandom(0);
        }

        /// <summary>
        /// Returns a random number greater than or equal to the specified minValue and less than the integer
        /// </summary>
        /// <param name="maxValue">The non-inclusive maximum integer value for the random number</param>
        /// <param name="minValue">the inclusive minimum integer value for the random number</param>
        /// <returns>A random integer</returns>
        public static int GetRandom(this int maxValue, int minValue)
        {
            return _rnd.Next(minValue, maxValue);
        }
    }
}
