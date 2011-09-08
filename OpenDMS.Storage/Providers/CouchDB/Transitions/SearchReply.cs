using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class SearchReply
    {
        private bool VerifyDocumentIntegrity(Model.Document document, out string property)
        {
            JArray jray;
            JObject jobj;
            property = null;

            //if (!CheckPropertyExistance(document, "analyzer", out property)) return false;
            if (!CheckPropertyExistance(document, "etag", out property)) return false;
            if (!CheckPropertyExistance(document, "fetch_duration", out property)) return false;
            if (!CheckPropertyExistance(document, "limit", out property)) return false;
            //if (!CheckPropertyExistance(document, "plan", out property)) return false;
            if (!CheckPropertyExistance(document, "q", out property)) return false;
            if (!CheckPropertyExistance(document, "rows", out property)) return false;
            if (!CheckPropertyExistance(document, "search_duration", out property)) return false;
            if (!CheckPropertyExistance(document, "skip", out property)) return false;
            if (!CheckPropertyExistance(document, "total_rows", out property)) return false;

            jray = (JArray)document["rows"];

            for (int i = 0; i < jray.Count; i++)
            {
                jobj = (JObject)jray[i];
                if (!CheckPropertyExistance(jobj, "id", out property)) return false;
                if (!CheckPropertyExistance(jobj, "score", out property)) return false;
                if (!CheckPropertyExistance(jobj, "fields", out property)) return false;
                jobj = (JObject)jobj["fields"];
                if (!CheckPropertyExistance(jobj, "type", out property)) return false;
                if (!CheckPropertyExistance(jobj, "created", out property)) return false;
                if (!CheckPropertyExistance(jobj, "creator", out property)) return false;
                if (!CheckPropertyExistance(jobj, "modified", out property)) return false;
                if (!CheckPropertyExistance(jobj, "modifier", out property)) return false;
                if (jobj["type"].Value<string>() == "resource")
                {
                    if (!CheckPropertyExistance(jobj, "tags", out property)) return false;
                    if (!CheckPropertyExistance(jobj, "checkedoutat", out property)) return false;
                    if (!CheckPropertyExistance(jobj, "checkedoutto", out property)) return false;
                    if (!CheckPropertyExistance(jobj, "lastcommit", out property)) return false;
                    if (!CheckPropertyExistance(jobj, "lastcommitter", out property)) return false;
                    if (!CheckPropertyExistance(jobj, "title", out property)) return false;
                }
                else if (jobj["type"].Value<string>() == "version")
                {
                    if (!CheckPropertyExistance(jobj, "md5", out property)) return false;
                    if (!CheckPropertyExistance(jobj, "extension", out property)) return false;
                }
                else
                    throw new UnsupportedException("The received type of document is not supported.");
            }

            return true;
        }

        private bool CheckPropertyExistance(JObject jobj, string property, out string propertyName)
        {
            propertyName = null;

            if (jobj[property] == null)
            {
                propertyName = property;
                return false;
            }

            return true;
        }

        private bool CheckPropertyExistance(Model.Document document, string property, out string propertyName)
        {
            propertyName = null;

            if (document[property] == null)
            {
                propertyName = property; 
                return false;
            }

            return true;
        }

        public SearchProviders.CdbLucene.SearchReply Transition(Model.Document document)
        {
            JArray jray;
            JObject jobj;
            string id, type;
            decimal score;
            SearchProviders.CdbLucene.SearchReply cdbSearchReply;
            SearchProviders.CdbLucene.ResourceResult cdbResourceResult;
            SearchProviders.CdbLucene.VersionResult cdbVersionResult;

            string verifyString;

            if (!VerifyDocumentIntegrity(document, out verifyString))
            {
                Logger.Storage.Error("The document is not properly formatted.  It is missing the property '" + verifyString + "'.");
                throw new FormattingException("The argument document does not have the necessary property '" + verifyString + "'.");
            }

            try
            {
                cdbSearchReply = new SearchProviders.CdbLucene.SearchReply()
                {
                    ETag = document["etag"].Value<string>(),
                    FetchDuration = document["fetch_duration"].Value<int>(),
                    Limit = document["limit"].Value<int>(),
                    Q = document["q"].Value<string>(),
                    SearchDuration = document["search_duration"].Value<int>(),
                    Skip = document["skip"].Value<int>(),
                    TotalRows = document["total_rows"].Value<int>()
                };

                // Optional
                if (document["analyzer"] != null) cdbSearchReply.Analyzer = document["analyzer"].Value<string>();
                if (document["plan"] != null) cdbSearchReply.Plan = document["plan"].Value<string>();
                
                jray = (JArray)document["rows"];

                for (int i = 0; i < jray.Count; i++)
                {

                    jobj = (JObject)jray[i];
                    id = jobj["id"].Value<string>();
                    score = jobj["score"].Value<decimal>();
                    jobj = (JObject)jobj["fields"];
                    type = jobj["type"].Value<string>();
                    if (type == "resource")
                    {
                        string abc = jobj["tags"].Value<string>();
                        string[] tagsArray = jobj["tags"].Value<string>().Split(',');
                        string[] rightsArray = jobj["usagerights"].Value<string>().Split(',');
                        List<string> tags = new List<string>();
                        List<Security.UsageRight> usageRights = new List<Security.UsageRight>();
                        
                        for (int j=0; j < tagsArray.Length; j++)
                        {
                            tagsArray[j] = tagsArray[j].Trim();
                            if (tagsArray[j].Length > 0)
                                tags.Add(tagsArray[j]);
                        }
                        
                        for (int j=0; j < rightsArray.Length; j++)
                        {
                            rightsArray[j] = rightsArray[j].Trim();
                            if (rightsArray[j].Length > 0)
                            {
                                string key = rightsArray[j].Substring(0, rightsArray[j].IndexOf(':'));
                                string value = rightsArray[j].Substring(rightsArray[j].IndexOf(':') + 1);

                                usageRights.Add(new Security.UsageRight(key, (Security.Authorization.ResourcePermissionType)int.Parse(value)));
                            }
                        }

                        cdbResourceResult = new SearchProviders.CdbLucene.ResourceResult(id, score, jobj["created"].Value<DateTime>(), 
                            jobj["creator"].Value<string>(), jobj["modified"].Value<DateTime>(),
                            jobj["modifier"].Value<string>(), jobj["checkedoutat"].Value<DateTime?>(),
                            jobj["checkedoutto"].Value<string>(), jobj["lastcommit"].Value<DateTime>(),
                            jobj["lastcommitter"].Value<string>(), jobj["title"].Value<string>(), tags, usageRights);
                        cdbSearchReply.Results.Add(cdbResourceResult);
                    }
                    else if (type == "version")
                    {
                        cdbVersionResult = new SearchProviders.CdbLucene.VersionResult(id, score,jobj["created"].Value<DateTime>(), 
                            jobj["creator"].Value<string>(), jobj["modified"].Value<DateTime>(),
                            jobj["modifier"].Value<string>(), jobj["md5"].Value<string>(), jobj["extension"].Value<string>());
                        cdbSearchReply.Results.Add(cdbVersionResult);
                    }
                    else
                        throw new UnsupportedException("The received type of document is not supported.");
                }
            }
            catch(Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }

            return cdbSearchReply;
        }
    }
}
