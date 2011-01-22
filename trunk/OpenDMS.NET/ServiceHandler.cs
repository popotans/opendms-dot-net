using System;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Collections.Generic;
using Common.NetworkPackage;

namespace OpenDMS
{
    public class ServiceHandler : IDisposable
    {
        private Storage.Master _storage;
        private Common.FileSystem.IO _fileSystem;
        private Common.Logger _generalLogger;
        private Common.Logger _networkLogger;

        public ServiceHandler()
        {
            if(!System.IO.Directory.Exists(@"C:\DataStore\logs"))
                System.IO.Directory.CreateDirectory(@"C:\DataStore\logs");
            _generalLogger = new Common.Logger(@"C:\DataStore\logs\", "GeneralLog.txt");
            _networkLogger = new Common.Logger(@"C:\DataStore\logs\", "NetworkLog.txt");
            _fileSystem = new Common.FileSystem.IO(@"C:\DataStore\", _generalLogger);
            _storage = new Storage.Master(_fileSystem, _generalLogger);
        }

        [ServicePoint("/_ping", ServicePointAttribute.VerbType.GET)]
        public void Ping(HttpApplication app)
        {
            new ServerResponse(true, ServerResponse.ErrorCode.None, "PONG").Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();
        }

        [ServicePoint("/_stats", ServicePointAttribute.VerbType.GET)]
        public void Stats(HttpApplication app)
        {
            throw new NotImplementedException();
        }

        [ServicePoint("/_lock", ServicePointAttribute.VerbType.PUT)]
        public void Lock(HttpApplication app)
        {
            ServerResponse resp;
            string errorMessage;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (queryString["releaselock"] == "true")
            {
                if (_storage.ReleaseLock(guid, userInfo["username"], out errorMessage) != Storage.ResultType.Success)
                {
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                }
                else
                {
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The resource was successfully unlocked.");
                }
            }
            else
            {
                if (_storage.ApplyLock(guid, userInfo["username"], out errorMessage) != Storage.ResultType.Success)
                {
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                }
                else
                {
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The resource was successfully locked.");
                }
            }

            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();
            return;       
        }

        [ServicePoint("/_settings/searchform", ServicePointAttribute.VerbType.GET)]
        public void GetSearchForm(HttpApplication app)
        {
            SearchForm searchForm;
            ServerResponse resp;
            string errorMessage;
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "Request for search form was received from user '" + userInfo["username"] + "'.");

            if (_storage.GetSearchForm(userInfo["username"], out searchForm, out errorMessage) != Storage.ResultType.Success)
            {
                if (_generalLogger != null)
                    _generalLogger.Write(Common.Logger.LevelEnum.Normal, 
                        "An error occurred while attempting to load the search form to respond to the request from user '" + userInfo["username"] + "'.\r\n" + errorMessage);

                resp = new ServerResponse(false, ServerResponse.ErrorCode.None, "Failed to load the search form template.");
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug, "Search form error message has been sent to user '" + userInfo["username"] + "'.");

                return;
            }

            searchForm.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "Search form was sent to user '" + userInfo["username"] + "'.");
        }

        [ServicePoint("/search", ServicePointAttribute.VerbType.GET)]
        public void Search(HttpApplication app)
        {
            OpenDMS.Search search;
            Common.NetworkPackage.SearchResult result;
            ServerResponse response;
            Dictionary<string, string> userInfo = ParseUserInfo(app);

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "Search request received from user '" + userInfo["username"] + "'.");

            search = new OpenDMS.Search(app.Request.Url.Query, userInfo["username"], _storage);

            result = search.Execute(_generalLogger, _networkLogger, out response);

            if (response != null)
            {
                // Error
                response.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                        "Error response for search request has been sent to user '" + userInfo["username"] + "'.");

                return;
            }

            result.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                    "Response for search request has been sent to user '" + userInfo["username"] + "'.");
        }

        [ServicePoint("/meta", ServicePointAttribute.VerbType.HEAD)]
        public void Head(HttpApplication app)
        {
            Common.Logger.LevelEnum logLevel;
            string errorMessage;
            ServerResponse resp;
            Common.Data.MetaAsset ma;
            Storage.ResultType result;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "Head request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            result = _storage.GetMeta(guid, userInfo["username"], true, out ma, out errorMessage);

            // If unsuccessful and not found -> kick it out
            if (result != Storage.ResultType.Success && result != Storage.ResultType.NotFound)
            {
                logLevel = Common.Logger.LevelEnum.Normal;
                switch (result)
                {
                    case Storage.ResultType.IOError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.SerializationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.InstantiationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.ResourceIsLocked:
                        logLevel = Common.Logger.LevelEnum.Debug;
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                        break;
                    default:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                }

                if (_generalLogger != null)
                    _generalLogger.Write(logLevel,
                        "An " + result.ToString() + " occurred while attempting to load the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                        "An error response for Head request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            app.Response.AppendHeader("ETag", ma.ETag.Value);
            app.CompleteRequest();

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                    "Response for Head request has been sent for id " + guid.ToString("N") + ", the ETag value was " + ma.ETag.Value.ToString() + " for user '" + userInfo["username"] + "'.");

            return;
        }

        [ServicePoint("/meta", ServicePointAttribute.VerbType.GET)]
        public void GetMeta(HttpApplication app)
        {
            Common.Logger.LevelEnum logLevel;
            Storage.ResultType result;
            ServerResponse resp;
            string errorMessage;
            Common.Data.MetaAsset ma;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "GetMeta request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            if (queryString["readonly"] == "true")
                result = _storage.GetMeta(guid, userInfo["username"], true, out ma, out errorMessage);
            else
                result = _storage.GetMeta(guid, userInfo["username"], false, out ma, out errorMessage);

            // If unsuccessful -> kick it out
            if (result != Storage.ResultType.Success)
            {
                logLevel = Common.Logger.LevelEnum.Normal;
                switch (result)
                {
                    case Storage.ResultType.NotFound:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                        break;
                    case Storage.ResultType.IOError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.SerializationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.InstantiationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.ResourceIsLocked:
                        logLevel = Common.Logger.LevelEnum.Debug;
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                        break;
                    default:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                }

                if (_generalLogger != null)
                    _generalLogger.Write(logLevel,
                        "An " + result.ToString() + " occurred while attempting to load the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                        "An error response for GetMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Send it to the user.
            ma.ExportToNetworkRepresentation().Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                    "Response for GetMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

            return;
        }

        [ServicePoint("/meta", ServicePointAttribute.VerbType.PUT)]
        public void SaveMeta(HttpApplication app)
        {
            Common.Logger.LevelEnum logLevel = Common.Logger.LevelEnum.Debug;
            Storage.ResultType result;
            ServerResponse resp;
            Common.Data.MetaAsset ma = null;
            Index index = new Index();
            string errorMessage;
            MetaAsset netMa = new MetaAsset();
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "SaveMeta request received for " + guid.ToString("N")  + " by user '" + userInfo["username"] + "'.");

            try
            {
                netMa.Deserialize(app.Request.InputStream);

                if (_generalLogger != null)
                    _generalLogger.Write(Common.Logger.LevelEnum.Debug, "Meta asset with id " + guid.ToString("N") + " was successfully deserialized for user '" + userInfo["username"] + "'.");
            }
            catch(Exception e)
            {
                if(_generalLogger != null)
                    _generalLogger.Write(Common.Logger.LevelEnum.Normal, "An exception occurred while attempting to deserialize the received meta asset.\r\n" + Common.Logger.ExceptionToString(e));

                resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, e.Message);
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if(_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug, "An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Create the MetaAsset
            try
            {
                ma = Common.Data.MetaAsset.Create(netMa, _fileSystem, _generalLogger);
            }
            catch (Exception e)
            {
                if (_generalLogger != null)
                    _generalLogger.Write(Common.Logger.LevelEnum.Normal, "An exception occurred while attempting to instantiate a meta asset based on the received meta asset.\r\n" + Common.Logger.ExceptionToString(e));

                resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, e.Message);
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug, "An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Make sure the Guid of the Request.Path matches the Guid in the package
            if (guid != ma.Guid)
            {
                if (_generalLogger != null)
                    _generalLogger.Write(Common.Logger.LevelEnum.Normal, "The resource id in the url did not match the guid in the meta asset package.");

                resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidGuid,
                    "The resource id in the url did not match the guid in the package, these values must match.");
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug, "An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Save it
            if(queryString["releaselock"] == "true")
                result = _storage.SaveMeta(ma, userInfo["username"], true, out errorMessage);
            else
                result = _storage.SaveMeta(ma, userInfo["username"], false, out errorMessage);

            // Define the result
            switch (result)
            {
                case Storage.ResultType.NotFound:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                    break;
                case Storage.ResultType.Success:
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The MetaAsset was successfully saved.");
                    break;
                case Storage.ResultType.PermissionsError:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidPermissions, errorMessage);
                    break;
                case Storage.ResultType.ResourceIsLocked:
                    logLevel = Common.Logger.LevelEnum.Normal;
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                    break;
                default:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                    break;
            }

            if (!(bool)resp["Pass"])
            {
                if (_generalLogger != null)
                    _generalLogger.Write(logLevel,
                        "An " + result.ToString() + " occurred while attempting to save the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug, "An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            if (!index.IndexMeta(ma, _fileSystem))
            {
                if (_generalLogger != null)
                    _generalLogger.Write(Common.Logger.LevelEnum.Normal,
                        "Failed to index the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp = new ServerResponse(false, ServerResponse.ErrorCode.FailedIndexing,
                    "The MetaAsset could not be indexed.");
            }

            // Send the result
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "Response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");
        }
        
        [ServicePoint("/data", ServicePointAttribute.VerbType.GET)]
        public void GetData(HttpApplication app)
        {
            Common.Logger.LevelEnum logLevel;
            Common.FileSystem.IOStream iostream;
            Storage.ResultType result;
            ServerResponse resp;
            string errorMessage;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "GetData request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            result = _storage.GetData(guid, userInfo["username"], (queryString["readonly"] == "true"), 
                out errorMessage, out iostream);
            
            // If unsuccessful -> kick it out
            if (result != Storage.ResultType.Success)
            {
                logLevel = Common.Logger.LevelEnum.Normal;
                switch (result)
                {
                    case Storage.ResultType.NotFound:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                        break;
                    case Storage.ResultType.IOError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.SerializationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.InstantiationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.ResourceIsLocked:
                        logLevel = Common.Logger.LevelEnum.Debug;
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                        break;
                    default:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                }

                if (_generalLogger != null)
                    _generalLogger.Write(logLevel,
                        "An " + result.ToString() + " occurred while attempting to load the data asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                        "An error response for GetData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            byte[] buffer = new byte[Common.ServerSettings.Instance.NetworkBufferSize];
            int bytesRead = 0;

            // Send it to the user.
            while ((bytesRead = iostream.Read(buffer, buffer.Length)) > 0)
            {
                app.Response.OutputStream.Write(buffer, 0, bytesRead);
            }

            _fileSystem.Close(iostream);

            app.CompleteRequest();

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug,
                    "Response for GetData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

            return;
        }

        [ServicePoint("/data", ServicePointAttribute.VerbType.PUT)]
        public void SaveData(HttpApplication app)
        {
            // Indexing example
            // curl "http://localhost:8080/solr/update/extract?literal.id=05d51eda96a269c9d474578cb300242d&literal.extension=.odt&literal.title=Test2&literal.tags=tag2&literal.tags=tag1&commit=true" -F "myfile=@05d51eda96a269c9d474578cb300242d.odt"

            Common.Logger.LevelEnum logLevel = Common.Logger.LevelEnum.Debug;
            Common.Data.MetaAsset ma;
            ServerResponse resp;
            Storage.ResultType result;
            Index index = new Index();
            string errorMessage;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "SaveData request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            if (queryString["releaselock"] == "true")
                result = _storage.SaveData(guid, app.Request.InputStream, userInfo["username"], true, out ma, out errorMessage);
            else
                result = _storage.SaveData(guid, app.Request.InputStream, userInfo["username"], false, out ma, out errorMessage);

            // Define the result
            switch (result)
            {
                case Storage.ResultType.NotFound:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                    break;
                case Storage.ResultType.Success:
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The DataAsset was successfully saved.");
                    break;
                case Storage.ResultType.PermissionsError:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidPermissions, errorMessage);
                    break;
                case Storage.ResultType.ResourceIsLocked:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                    break;
                default:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                    break;
            }

            if (!(bool)resp["Pass"])
            {
                if (_generalLogger != null)
                    _generalLogger.Write(logLevel,
                        "An " + result.ToString() + " occurred while attempting to save the data asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                if (_networkLogger != null)
                    _networkLogger.Write(Common.Logger.LevelEnum.Debug, "An error response for the SaveData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            if (!index.IndexData(guid, "data\\" + guid.ToString("N") + ma.Extension, _fileSystem))
            {
                if (_generalLogger != null)
                    _generalLogger.Write(Common.Logger.LevelEnum.Normal,
                        "Failed to index the data asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp = new ServerResponse(false, ServerResponse.ErrorCode.FailedIndexing,
                    "The DataAsset could not be indexed.");
            }

            // Send the result
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            if (_networkLogger != null)
                _networkLogger.Write(Common.Logger.LevelEnum.Debug, "Response for the SaveData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");
        }

        /// <summary>
        /// Test1 creates a new resource (or a new version thereof) every time it is called
        /// </summary>
        /// <param name="app"></param>
        [ServicePoint("/_test1", ServicePointAttribute.VerbType.ALL)]
        public void Test1(HttpApplication app)
        {
            FileStream fs;
            string errorMessage, md5;
            Common.Data.MetaAsset ma;
            System.Collections.Generic.List<string> tags = new System.Collections.Generic.List<string>();
            Dictionary<string, object> uprop = new Dictionary<string,object>();
            tags.Add("tag1");
            tags.Add("tag2");
            tags.Add("tag3");
            uprop.Add("prop1", 1);
            uprop.Add("prop2", 1L);
            uprop.Add("prop3", 1D);
            uprop.Add("prop4", "string value");
            uprop.Add("prop5", DateTime.Now);


            md5 = _fileSystem.ComputeMd5(@"C:\ClientDataStore\Data\05d51eda96a269c9d474578cb300242d.odt");

            ma = Common.Data.MetaAsset.Create(new Guid("05d51eda96a269c9d474578cb300242d"), 
                new Common.Data.ETag("1"), 1, 1, "lucas", DateTime.Now, 
                "Lucas J. Nodine", 26703, md5, ".odt", DateTime.Now, DateTime.Now, DateTime.Now, 
                "Some document", tags, uprop, _fileSystem, _generalLogger);
            
            if (_storage.SaveMeta(ma, "lucas", false, out errorMessage) != Storage.ResultType.Success)
            {
                app.Response.Write("Failed - " + errorMessage);
                app.CompleteRequest();
                return;
            }

            Index index = new Index();
            if(!index.IndexMeta(ma, _fileSystem))
            {
                app.Response.Write("Failed to index MetaAsset.");
                app.CompleteRequest();
                return;
            }

            fs = new FileStream(@"C:\ClientDataStore\Data\05d51eda96a269c9d474578cb300242d.odt", FileMode.Open, FileAccess.Read, FileShare.None);
            if (_storage.SaveData(ma.Guid, fs, "lucas", true, out ma, out errorMessage) != Storage.ResultType.Success)
            {
                app.Response.Write("Failed - " + errorMessage);
                app.CompleteRequest();
                return;
            }

            if (!index.IndexData(ma.Guid, "data\\" + ma.Guid.ToString("N") + ma.Extension, _fileSystem))
            {
                app.Response.Write("Failed to index DataAsset.");
                app.CompleteRequest();
                return;
            }

            app.Response.Write("Success");
            app.CompleteRequest();
        }

        private Dictionary<string, string> ParseUserInfo(HttpApplication app)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("username", app.Request.Headers.Get("x-lo-user"));
            dict.Add("password", app.Request.Headers.Get("x-lo-pass"));

            if (string.IsNullOrEmpty(dict["username"])) dict["username"] = "lucas";
            if (string.IsNullOrEmpty(dict["password"])) dict["password"] = "nodine";

            return dict;
        }

        private Guid ParseGuid(string path)
        { 
            // http://stackoverflow.com/questions/104850/c-test-if-string-is-a-guid-without-throwing-exceptions
            // expect many less exceptions
            try
            {
                return new Guid(path.Substring(path.LastIndexOf("/")+1).Replace(".xml", ""));
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        private Dictionary<string, string> ParseQueryString(HttpApplication app)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("releaselock", null);
            dict.Add("version", null);
            dict.Add("readonly", null);

            if (!app.Request.QueryString.HasKeys())
                return dict;

            if (!string.IsNullOrEmpty(app.Request.QueryString["releaselock"]))
                dict["releaselock"] = app.Request.QueryString["releaselock"];
            if (!string.IsNullOrEmpty(app.Request.QueryString["version"]))
                dict["version"] = app.Request.QueryString["version"];
            if (!string.IsNullOrEmpty(app.Request.QueryString["readonly"]))
                dict["readonly"] = app.Request.QueryString["readonly"];

            return dict;
        }

        public void Dispose()
        {
            _storage = null;
            _fileSystem = null;
            _networkLogger.Close();
            _generalLogger.Close();
        }
    }
}
