using System;
using System.Collections.Generic;

namespace OpenDMS.Security
{
    public class UserRightCollection 
        : Dictionary<string, int>
    {
        public byte[] ToBytes()
        {
            string total = "";
            Dictionary<string, int>.Enumerator en = GetEnumerator();

            while (en.MoveNext())
            {
                total += en.Current.Key + ":" + en.Current.Value.ToString() + ",";
            }

            if (total.Length > 0)
                total = total.Trim(new char[] { ',' });

            return System.Text.Encoding.UTF8.GetBytes(total);
        }

        public void FromString(string str)
        {
            string[] vals;
            string[] entries = str.Split(',');

            foreach (string entry in entries)
            {
                vals = entry.Split(':');
                Add(vals[0], int.Parse(vals[1]));
            }
        }
    }
}
