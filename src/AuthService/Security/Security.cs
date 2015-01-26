using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Security
{
    public class Security
    {
        // The following constants may be changed without breaking existing hashes.
        /// <summary>
        /// The SAL t_ BYTES
        /// </summary>
        private const int SALT_BYTES = 48;

        /// <summary>
        /// The HAS h_ BYTES
        /// </summary>
        private const int HASH_BYTES = 48;

        /// <summary>
        /// The PBKD F2_ ITERATIONS
        /// </summary>
        private const int PBKDF2_ITERATIONS = 1000;

        /// <summary>
        /// The ITERATIO n_ INDEX
        /// </summary>
        private const int ITERATION_INDEX = 0;

        /// <summary>
        /// The SAL t_ INDEX
        /// </summary>
        private const int SALT_INDEX = 1;

        /// <summary>
        /// The PBKD F2_ INDEX
        /// </summary>
        private const int PBKDF2_INDEX = 2;

        /// <summary>
        /// Creates the hash.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public string CreateHash(string password)
        {
            // Generate a random salt
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SALT_BYTES];
            csprng.GetBytes(salt);

            // Hash the password and encode the parameters
            byte[] hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTES);
            return PBKDF2_ITERATIONS + ":" +
                   Convert.ToBase64String(salt) + ":" +
                   Convert.ToBase64String(hash);
        }



        /// <summary>
        /// Validates the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="hash">The hash.</param>
        /// <returns></returns>
        public bool ValidatePassword(string password, string goodHash)
        {
            // Extract the parameters from the hash
            char[] delimiter = { ':' };
            string[] split = goodHash.Split(delimiter);
            int iterations = Int32.Parse(split[ITERATION_INDEX]);
            byte[] salt = Convert.FromBase64String(split[SALT_INDEX]);
            byte[] hash = Convert.FromBase64String(split[PBKDF2_INDEX]);

            byte[] testHash = PBKDF2(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        /// <summary>
        /// Slows the equals.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        /// <summary>
        /// PBKDs the f2.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The iterations.</param>
        /// <param name="outputBytes">The output bytes.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt);
            pbkdf2.IterationCount = iterations;
            return pbkdf2.GetBytes(outputBytes);
        }
    }
}
