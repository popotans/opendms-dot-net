using System.Net;

namespace OpenDMS.Networking
{
    public class Utilities
    {
		#region Methods (5) 

		// Public Methods (5) 

        public static int ConvertHexToInt(string hexValue)
        {
            try
            {
                return int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                return 0;
            }
        }

        public static ulong GetContentLength(WebHeaderCollection headers)
        {
            if (headers != null &&
                headers["Content-Length"] != null)
            {
                return ulong.Parse(headers["Content-Length"]);
            }
            return 0;
        }

        public static string GetContentType(WebHeaderCollection headers)
        {

            if (headers != null &&
                headers["Content-Type"] != null)
                return headers["Content-Type"];
            else
                return null;

        }

        public static string GetTransferEncoding(WebHeaderCollection headers)
        {
            if (headers != null &&
                headers["Transfer-Encoding"] != null)
                return headers["Transfer-Encoding"];
            else
                return null;
        }

        /// <summary>
        /// Converts a <see cref="System.IO.Stream"/> to a UTF-8 formatted string.
        /// </summary>
        /// <param name="stream">The <see cref="System.IO.Stream"/>.</param>
        /// <returns>A UTF-8 formatted string.</returns>
        public static string StreamToUtf8String(System.IO.Stream stream)
        {
            long startPos;
            int bytesRead = 0;
            byte[] buffer = new byte[40960];
            string str = "";

            if (!stream.CanRead)
                throw new System.IO.IOException("Cannot read from stream.");

            if (!stream.CanSeek)
                throw new System.IO.IOException("Cannot reposition stream.");

            startPos = stream.Position;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                str += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

            stream.Seek(startPos, System.IO.SeekOrigin.Begin);

            return str;
        }

		#endregion Methods 
    }
}
