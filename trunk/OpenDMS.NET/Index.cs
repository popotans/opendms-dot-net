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
using System.Collections.Generic;
using System.Net;

namespace OpenDMS
{
    public class Index
    {
        public bool IndexMeta(Common.Data.MetaAsset ma, Common.FileSystem.IO fileSystem)
        {
            HttpWebResponse response;
            List<KeyValuePair<string, string>> postParameters = new List<KeyValuePair<string, string>>();

            postParameters.Add(new KeyValuePair<string,string>("literal.id", ma.GuidString + ".meta"));
            postParameters.Add(new KeyValuePair<string,string>("literal.creator", ma.Creator));
            postParameters.Add(new KeyValuePair<string,string>("literal.extension", ma.Extension));
            postParameters.Add(new KeyValuePair<string,string>("literal.title", ma.Title));
            postParameters.Add(new KeyValuePair<string, string>("literal.last_modified", ma.Modified.ToUniversalTime().ToString("s") + "Z"));
            
            // tags
            for (int i = 0; i < ma.Tags.Count; i++)
                postParameters.Add(new KeyValuePair<string,string>("literal.tags", ma.Tags[i]));

            // This adds some burden to commit immediately instead of batching all requests
            // however, it makes the resource immediately available for searching
            postParameters.Add(new KeyValuePair<string,string>("commit", "true"));

            // Add file
            postParameters.Add(new KeyValuePair<string, string>(ma.GuidString + ".xml",
                "file://" + Common.Data.AssetType.Meta.VirtualPath + "\\" + ma.GuidString + ".xml"));

            Common.NetworkPackage.ServerResponse resp;

            response = Common.Network.MultipartFormMessage.Send("http://" +
                Settings.Instance.SearchHost.Address.ToString() + ":" +
                Settings.Instance.SearchHost.Port.ToString() + "/solr/update/extract", postParameters,
                fileSystem, out resp);

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }

        public bool IndexData(Guid guid, string relativeFilepath, Common.FileSystem.IO fileSystem)
        {
            HttpWebResponse response;
            List<KeyValuePair<string, string>> postParameters = new List<KeyValuePair<string, string>>();

            postParameters.Add(new KeyValuePair<string, string>("literal.id", System.IO.Path.GetFileName(relativeFilepath)));

            // This adds some burden to commit immediately instead of batching all requests
            // however, it makes the resource immediately available for searching
            postParameters.Add(new KeyValuePair<string, string>("commit", "true"));

            // Add file
            postParameters.Add(new KeyValuePair<string, string>(System.IO.Path.GetFileName(relativeFilepath),
                "file://" + relativeFilepath));

            Common.NetworkPackage.ServerResponse resp;

            response = Common.Network.MultipartFormMessage.Send("http://" +
                Settings.Instance.SearchHost.Address.ToString() + ":" +
                Settings.Instance.SearchHost.Port.ToString() + "/solr/update/extract", postParameters,
                fileSystem, out resp);

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }
    }
}
