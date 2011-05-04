using System;
using System.Net;

namespace Common.Http
{
    public class Utilities
    {
        public static string GetContentType(WebHeaderCollection headers)
        {

            if (headers != null &&
                headers["Content-Type"] != null)
                return headers["Content-Type"];
            else
                return null;

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

        public static string GetTransferEncoding(WebHeaderCollection headers)
        {
            if (headers != null &&
                headers["Transfer-Encoding"] != null)
                return headers["Transfer-Encoding"];
            else
                return null;
        }

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
    }
}
