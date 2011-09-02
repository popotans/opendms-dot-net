using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;
using OpenDMS.Storage.Data;

namespace StorageTesting
{
    public class CreateNewResource : TestBase
    {
        private DateTime _start;

        public CreateNewResource(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            OpenDMS.Storage.Providers.EngineRequest request = new OpenDMS.Storage.Providers.EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.OnActionChanged += new EngineBase.ActionDelegate(EngineAction);
            request.OnProgress += new EngineBase.ProgressDelegate(Progress);
            request.OnComplete += new EngineBase.CompletionDelegate(Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.AuthToken = _window.Session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.User;

            Clear();

            OpenDMS.Storage.Providers.CreateResourceArgs resourceArgs = new CreateResourceArgs()
            {
                VersionArgs = new CreateVersionArgs()
            };

            resourceArgs.Metadata = new OpenDMS.Storage.Data.Metadata();
            resourceArgs.Tags = new List<string>();
            resourceArgs.Tags.Add("Tag1");
            resourceArgs.Tags.Add("Tag2");
            resourceArgs.Title = "Test resource";
            resourceArgs.VersionArgs.Extension = "txt";
            resourceArgs.VersionArgs.Metadata = new OpenDMS.Storage.Data.Metadata();


            System.IO.FileStream fs = new System.IO.FileStream("testdoc.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 8192, System.IO.FileOptions.None);
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("This is a test content file.");
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();

            fs = new System.IO.FileStream("testdoc.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None, 8192, System.IO.FileOptions.None);
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] data = md5.ComputeHash(fs);
            string output = "";
            fs.Close();
            md5.Dispose();
            fs.Dispose();

            for (int i = 0; i < data.Length; i++)
                output += data[i].ToString("x2");


            resourceArgs.VersionArgs.Md5 = output;
            resourceArgs.VersionArgs.Content = new OpenDMS.Storage.Data.Content(bytes.Length, new OpenDMS.Storage.Data.ContentType("text/plain"), "testdoc.txt");



            WriteLine("Starting CreateNewResource test...");
            _start = DateTime.Now;

            _engine.CreateNewResource(request, resourceArgs);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("CreateNewResource.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("CreateNewResource.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("CreateNewResource.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            Tuple<Resource, OpenDMS.Storage.Data.Version> r = (Tuple<Resource, OpenDMS.Storage.Data.Version>)result;
            WriteLine("CreateNewResource.Complete - Resource: " + r.Item1.ResourceId.ToString() + " and Version: " + r.Item2.VersionId.ToString() + " successfully created in " + duration.TotalMilliseconds.ToString() + "ms.");
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("CreateNewResource.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("CreateNewResource.Error - Error.  Message: " + message);
        }
    }
}
