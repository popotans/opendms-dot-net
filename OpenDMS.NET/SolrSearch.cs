using System;
using System.Xml;
using System.Collections.Generic;

namespace OpenDMS
{
    public class SolrSearch
    {
        private string _host;
        private int _port;

        public SolrSearch(string host, int port)
        {
            _host = host;
            _port = port;
        }

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
