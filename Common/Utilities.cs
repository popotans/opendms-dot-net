using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    class Utilities
    {
        /// <summary>
        /// Converts a <see cref="System.IO.Stream"/> to a UTF-8 formatted string.
        /// </summary>
        /// <param name="stream">The <see cref="System.IO.Stream"/>.</param>
        /// <returns>A UTF-8 formatted string.</returns>
        public static string StreamToUtf8String(System.IO.Stream stream)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[40960];
            string str = "";

            if (!stream.CanRead)
                throw new System.IO.IOException("Cannot read from stream.");

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                str += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return str;
        }
    }
}
