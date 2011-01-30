/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace Common.Network
{
    public class MultipartFormMessage
    {
        private readonly Encoding _encoding = Encoding.UTF8;

        private static string NewDataBoundary()
        {
            Random rnd = new Random();
            string formDataBoundary = "";
            while (formDataBoundary.Length < 15)
            {
                formDataBoundary = formDataBoundary + rnd.Next();
            }
            formDataBoundary = formDataBoundary.Substring(0, 15);
            formDataBoundary = "-----------------------------" + formDataBoundary;
            return formDataBoundary;
        }

        public static HttpWebResponse Send(string postUrl, 
            List<KeyValuePair<string, string>> postParameters, FileSystem.IO fileSystem,
            out Common.NetworkPackage.ServerResponse response)
        {
            string boundary;
            HttpWebRequest request;

            response = null;
            boundary = NewDataBoundary();
            request = (HttpWebRequest)WebRequest.Create(postUrl);

            // Set up the request properties
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            //request.UserAgent = "PhasDocAgent 1.0";
            
            using (Stream formDataStream = request.GetRequestStream())
            {
                foreach (KeyValuePair<string, string> item in postParameters)
                {
                    if (item.Value.StartsWith("file://"))
                    {
                        byte[] buffer = new byte[ServerSettings.Instance.NetworkBufferSize];
                        int bytesRead = 0;
                        FileSystem.IOStream iostream;
                        string filepath = item.Value.Substring(7);

                        // Add just the first part of this param, since we will write the file data directly to the Stream
                        //string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                        //    boundary,
                        //    param.Key,
                        //    Path.GetFileName(filepath) ?? param.Key,
                        //    MimeTypes.GetMime(filepath));
                        string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\n\r\n",
                            boundary,
                            item.Key,
                            Path.GetFileName(filepath));

                        formDataStream.Write(Encoding.UTF8.GetBytes(header), 0, header.Length);

                        // Write the file data directly to the Stream, rather than serializing it to a string.
                        iostream = fileSystem.Open(filepath, FileMode.Open, FileAccess.Read, 
                            FileShare.ReadWrite, FileOptions.None, buffer.Length, 
                            "Common.Network.MultipartFormMessage.Send()");

                        while ((bytesRead = iostream.Read(buffer, buffer.Length)) > 0)
                            formDataStream.Write(buffer, 0, bytesRead);

                        fileSystem.Close(iostream);
                    }
                    else
                    {
                        string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                            boundary,
                            item.Key,
                            item.Value);
                        formDataStream.Write(Encoding.UTF8.GetBytes(postData), 0, postData.Length);
                    }
                }
                // Add the end of the request
                byte[] footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                formDataStream.Write(footer, 0, footer.Length);
                //request.ContentLength = formDataStream.Length;
                formDataStream.Close();
            }

            HttpWebResponse resp = null;

            try
            {
                resp = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                string str = "";
                int bytesRead = 0;
                byte[] buffer = new byte[ServerSettings.Instance.NetworkBufferSize];
                Stream stream = e.Response.GetResponseStream();

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    str += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                response = new NetworkPackage.ServerResponse(false,
                    NetworkPackage.ServerResponse.ErrorCode.Exception,
                    str);

                return null;
            }
            finally
            {
                resp.Close();
            }

            return resp;
        }


        //private byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        //{
        //    Dictionary<string, object>.Enumerator en = postParameters.GetEnumerator();
        //    Stream formDataStream = new System.IO.MemoryStream();

        //    while(en.MoveNext())
        //    {
        //        if (en.Current.Value.GetType() == typeof(FileParameter))
        //        {
        //            FileParameter fileToUpload = (FileParameter)en.Current.Value;

        //            // Add just the first part of this param, since we will write the file data directly to the Stream
        //            string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
        //                boundary,
        //                en.Current.Key,
        //                fileToUpload.FileName ?? en.Current.Key,
        //                fileToUpload.ContentType ?? "application/octet-stream");

        //            formDataStream.Write(_encoding.GetBytes(header), 0, header.Length);

        //            // Write the file data directly to the Stream, rather than serializing it to a string.
        //            formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
        //        }
        //        else
        //        {
        //            string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
        //                boundary,
        //                en.Current.Key,
        //                en.Current.Value);
        //            formDataStream.Write(_encoding.GetBytes(postData), 0, postData.Length);
        //        }
        //    }

        //    // Add the end of the request
        //    string footer = "\r\n--" + boundary + "--\r\n";
        //    formDataStream.Write(_encoding.GetBytes(footer), 0, footer.Length);

        //    // Dump the Stream into a byte[]
        //    formDataStream.Position = 0;
        //    byte[] formData = new byte[formDataStream.Length];
        //    formDataStream.Read(formData, 0, formData.Length);
        //    formDataStream.Close();

        //    return formData;
        //}

        //public class FileParameter
        //{
        //    public string FilePath { get; set; }
        //    public string FileName { get; set; }
        //    public string ContentType { get; set; }
            
        //    public FileParameter(string filepath) : this(filepath, null) { }

        //    public FileParameter(string filepath, string filename) : this(filepath, filename, null) { }

        //    public FileParameter(string filepath, string filename, string contenttype)
        //    {
        //        FilePath = filepath;
        //        FileName = filename;
        //        ContentType = contenttype;
        //    }
        //}

    }
}
