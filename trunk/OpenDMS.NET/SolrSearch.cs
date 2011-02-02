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
using System.Xml;
using System.Collections.Generic;

namespace HttpModule
{
    /// <summary>
    /// Provides an interface for relaying a search request to a Solr instance.
    /// </summary>
    public class SolrSearch
    {
        /// <summary>
        /// The IP address of the Solr host.
        /// </summary>
        private string _host;
        /// <summary>
        /// The port number of the Solr host.
        /// </summary>
        private int _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolrSearch"/> class.
        /// </summary>
        /// <param name="host">The IP address of the Solr host.</param>
        /// <param name="port">The port number of the Solr host.</param>
        public SolrSearch(string host, int port)
        {
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Executes the specified search on the Solr instance.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <param name="generalLogger">A reference to the <see cref="Common.Logger"/> that this instance should use to document general events.</param>
        /// <param name="networkLogger">A reference to the <see cref="Common.Logger"/> that this instance should use to document network events.</param>
        /// <param name="response">The Solr instance response.</param>
        /// <returns>A collection of results.</returns>
        public List<SolrResult> Execute(string queryString, Common.Logger generalLogger, Common.Logger networkLogger,
            out Common.NetworkPackage.ServerResponse response)
        {
            List<SolrResult> results;
            Common.Network.Message msg = null;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] buffer = new byte[Common.ServerSettings.Instance.NetworkBufferSize];
            int bytesRead = 0;
            XmlReader reader = null;

            response = null;

            try
            {
                msg = new Common.Network.Message(_host, _port, "solr/select", queryString, 
                    Common.Network.OperationType.GET, Common.Network.DataStreamMethod.Memory, 
                    null, null, null, null, false, true, true, true,
                    Common.ServerSettings.Instance.NetworkBufferSize, Common.ServerSettings.Instance.NetworkTimeout, 
                    generalLogger, networkLogger);
            }
            catch (Exception)
            {
                response = new Common.NetworkPackage.ServerResponse(false, 
                    Common.NetworkPackage.ServerResponse.ErrorCode.Exception, 
                    "Could not create the search request.");
                return null;
            }

            try
            {
                msg.Send();
            }
            catch (Exception)
            {
                response = new Common.NetworkPackage.ServerResponse(false, 
                    Common.NetworkPackage.ServerResponse.ErrorCode.InvalidSearchParameters, 
                    "The search parameters provided were invalid.");
                return null;
            }

            queryString = "";

            while ((bytesRead = msg.State.Stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                queryString += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }

            msg.State.Dispose();

            ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(queryString));
            reader = XmlReader.Create(ms, new XmlReaderSettings() { IgnoreWhitespace = true,  });


            results = Deserialize(reader);

            reader.Close();

            return results;
        }

        /// <summary>
        /// Deserializes the content of the XML reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A collection of results.</returns>
        public List<SolrResult> Deserialize(XmlReader reader)
        {
            List<SolrResult> results = new List<SolrResult>();
            bool loop = true;
            
            if (reader.IsEmptyElement)
                return results;

            if (reader.NodeType == XmlNodeType.None)
                reader.MoveToContent();

            while (loop)
            {
                switch (reader.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        if (string.IsNullOrEmpty(reader.LocalName))
                            reader.MoveToContent();

                        switch (reader.LocalName)
                        {
                            case "response":
                                reader.ReadStartElement("response");
                                break;
                            case "lst":
                                reader.Skip();
                                break;
                            case "result":
                                reader.ReadStartElement("result");
                                break;
                            case "doc":
                                bool found = false;
                                reader.ReadStartElement("doc");
                                SolrResult res = SolrResult.Deserialize(reader);

                                // Make sure it does not already exist
                                for(int i=0; i<results.Count; i++)
                                {
                                    if(results[i].Id == res.Id)
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if(!found)
                                    results.Add(res);

                                reader.ReadEndElement();
                                break;
                            default:
                                break;
                        }


                        break;
                    case System.Xml.XmlNodeType.None:
                        loop = false;
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == "response")
                        {
                            reader.ReadEndElement();
                            return results;
                        }
                        else if (reader.LocalName == "result")
                            reader.ReadEndElement();
                        break;
                    default:
                        reader.MoveToContent();
                        break;
                }
            }

            return results;
        }
    }
}
