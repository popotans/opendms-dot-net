using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api
{
    public class CommandParser
    {
        private Stream _stream;
        private Http.HttpNetworkStream _networkStream;

        public long JsonLength { get; private set; }
        public JObject Json { get; private set; }
        public Stream Stream { get; private set; }
        public Http.HttpNetworkStream NetworkStream { get; private set; }

        public CommandParser(Stream stream)
        {
            _stream = stream;
        }

        public CommandParser(Http.HttpNetworkStream stream)
        {
            _networkStream = stream;
        }

        public void Execute()
        {
            if (_stream != null)
                ExecuteStream();
            else
                ExecuteNetworkStream();
        }

        private void ExecuteNetworkStream()
        {
            byte[] buffer = new byte[4192];
            int i = 0;
            int bytesRead = 0;
            int bytesToRead = 0;
            string szLength;
            long totalJsonBytesRead = 0;
            string json = "";

            // Read to \0 byte at a time
            while (true)
            {
                buffer[i] = (byte)_networkStream.ReadByte();
                if (buffer[i] < 1)
                    break; // length is what we have in szLength now
                i++;
            }

            szLength = System.Text.Encoding.UTF8.GetString(buffer, 0, i);
            JsonLength = long.Parse(szLength);

            if (JsonLength > buffer.Length)
                bytesToRead = buffer.Length;
            else
                bytesToRead = (int)JsonLength;

            while ((bytesRead = _networkStream.Read(buffer, 0, bytesToRead)) > 0)
            {
                totalJsonBytesRead += bytesRead;

                // if the totalJsonBytesRead plus another full read are <= JsonLength then run a full read
                // else run a read for the JsonLength - totalJsonBytesRead
                if (JsonLength - totalJsonBytesRead <= buffer.Length)
                    bytesToRead = (int)(JsonLength - totalJsonBytesRead);
                else
                    bytesToRead = buffer.Length;

                json += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }

            Json = JObject.Parse(json);
            NetworkStream = _networkStream;
        }

        private void ExecuteStream()
        {
            byte[] buffer = new byte[4192];
            int i=0;
            int bytesRead = 0;
            int bytesToRead = 0;
            string szLength;
            long totalJsonBytesRead = 0;
            string json = "";

            // Read to \0 byte at a time
            while (true)
            {
                buffer[i] = (byte)_stream.ReadByte();
                if (buffer[i] < 1)
                    break; // length is what we have in szLength now
                i++;
            }

            szLength = System.Text.Encoding.UTF8.GetString(buffer, 0, i);
            JsonLength = long.Parse(szLength);

            if (JsonLength > buffer.Length)
                bytesToRead = buffer.Length;
            else
                bytesToRead = (int)JsonLength;
            
            while ((bytesRead = _stream.Read(buffer, 0, bytesToRead)) > 0)
            {
                totalJsonBytesRead += bytesRead;

                // if the totalJsonBytesRead plus another full read are <= JsonLength then run a full read
                // else run a read for the JsonLength - totalJsonBytesRead
                if (JsonLength - totalJsonBytesRead <= buffer.Length)
                    bytesToRead = (int)(JsonLength - totalJsonBytesRead);
                else
                    bytesToRead = buffer.Length;

                json += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }

            Json = JObject.Parse(json);
            Stream = _stream;
        }
    }
}
