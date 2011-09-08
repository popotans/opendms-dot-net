using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

using Search = OpenDMS.Storage.SearchProviders.CdbLucene;

namespace StorageTesting
{
    public class SearchTest : TestBase
    {
        private DateTime _start;

        public SearchTest(FrmMain window, IEngine engine, IDatabase db)
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

            Search.Query query = new Search.Query();
            Search.Tokens.Field tags = new Search.Tokens.Field("tags");
            tags.Tokens.Add(new Search.Tokens.Term("tag1"));
            query.Add(tags);
            Search.Tokens.Field content = new Search.Tokens.Field("attachment");
            content.Tokens.Add(new Search.Tokens.Term("content"));
            query.Add(content);


            WriteLine("Starting SearchTest test...");
            _start = DateTime.Now;
            _engine.Search(request, new SearchArgs(query));
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("SearchTest.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("SearchTest.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("SearchTest.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            WriteLine("SearchTest.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("SearchTest.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("SearchTest.Error - Error.  Message: " + message);
        }

        private void QueryBuildingTest()
        {
            Search.Query query = new Search.Query();

            // The
            query.Add(new Search.Tokens.Term("The"));

            // The "big red"
            Search.Tokens.Phrase p1 = new Search.Tokens.Phrase();
            p1.Tokens.Add(new Search.Tokens.Term("big"));
            p1.Tokens.Add(new Search.Tokens.Term("red"));
            query.Add(p1);

            // The "big red" dog^2
            query.Add(new Search.Tokens.Term("dog", new Search.Modifiers.Boost(2)));

            // jumps OR over
            Search.Tokens.Group g1 = new Search.Tokens.Group();
            g1.Tokens.Add(new Search.Tokens.Term("jumps"));
            g1.Tokens.Add(new Search.Tokens.Term("over", new Search.Operators.Or()));

            // The "big red" dog^2 (jumps OR over)
            query.Add(g1);

            //"The \"big red\" dog^2 (jumps OR over) [the TO yellow]
            query.Add(new Search.Tokens.Range("the", "yellow"));

            //"The \"big red\" dog^2 (jumps OR over) [the TO yellow] NOT bone
            query.Add(new Search.Tokens.Term("bone", new Search.Operators.Not()));

            //"The \"big red\" dog^2 (jumps OR over) [the TO yellow] NOT bone "but falls"~10
            Search.Tokens.Phrase p2 = new Search.Tokens.Phrase();
            p2.Tokens.Add(new Search.Tokens.Term("but"));
            p2.Tokens.Add(new Search.Tokens.Term("falls"));
            p2.Modifiers.Add(new Search.Modifiers.Proximity(10));
            query.Add(p2);

            //"The \"big red\" dog^2 (jumps OR over) [the TO yellow] NOT bone "but falls"~10 t~en
            query.Add(new Search.Tokens.Term("t?en", new Search.Modifiers.SingleWildcard()));

            //"The \"big red\" dog^2 (jumps OR over) [the TO yellow] NOT bone "but falls"~10 t~en rolls~0.5
            query.Add(new Search.Tokens.Term("rolls", new Search.Modifiers.Fuzzy(0.5f)));

            //"The \"big red\" dog^2 (jumps OR over) [the TO yellow] NOT bone "but falls"~10 t~en rolls~0.5 d*
            query.Add(new Search.Tokens.Term("d*", new Search.Modifiers.MultiWildcard()));


            string s = null;
            try
            {
                s = query.ToString();
            }
            catch (Exception e)
            {
                string a = "";
            }
            WriteLine(s);
        }
    }
}
