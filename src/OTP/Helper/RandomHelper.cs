using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTP.Helper
{
    public class RandomHelper
    {

        /// <summary>
        /// Generates the random byte array. Used to generate
        /// the 20 bytes secret.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="array">The array.</param>
        public static void GenerateRandomByteArray(int length, out byte[] array)
        {
            array = new byte[length];
            Random random = new Random();
            random.NextBytes(array);
        }
    }
}
