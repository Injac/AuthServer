using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Storage
{
    /// <summary>
    /// System functions that help to
    /// calculate file-sizes.
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// Strings the size of the format byte.
        /// </summary>
        /// <param name="fileSize">Size of the file.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns></returns>
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(
            long fileSize
            , [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer
            , int bufferSize);

        /// <summary>
        /// Strings the size of the format byte.
        /// </summary>
        /// <param name="filesize">The filesize.</param>
        /// <returns></returns>
        public static string StrFormatByteSize(long filesize)
        {
            StringBuilder sb = new StringBuilder(11);
            StrFormatByteSize(filesize, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
