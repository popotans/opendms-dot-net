using System;
using Common.Http;
using Common.Http.Methods;
using Common.Http.Network;
using Common.Data.Providers.CouchDB.Commands;

namespace Common.Data.Providers.CouchDB
{
    public class BaseStorageObject : IStorageObject
    {
        public delegate void TimeoutDelegate(BaseStorageObject sender, Commands.Base command, Client client, HttpConnection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void ProgressDelegate(BaseStorageObject sender, Commands.Base command, Client client, HttpConnection connection, DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void ErrorDelegate(BaseStorageObject sender, Commands.Base command, Client client, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CompletionDlegate(BaseStorageObject sender, Commands.Base command, Client client, HttpConnection connection, HttpResponse response);
        public event CompletionDlegate OnComplete;

        protected Uri _uri = null;
        protected Commands.Base _baseCommand = null;

        public Uri Uri { get { return _uri; } }

        private Commands.Base AttachEvents(Commands.Base b)
        {
            b.OnComplete += new Base.CompletionDlegate(Base_OnComplete);
            b.OnError += new Base.ErrorDelegate(Base_OnError);
            b.OnProgress += new Base.ProgressDelegate(Base_OnProgress);
            b.OnTimeout += new Base.TimeoutDelegate(Base_OnTimeout);

            return b;
        }

        public Uri IStorageObject.Uri
        {
            get { return Uri; }
        }

        public virtual List IStorageObject.List
        {
            get { throw new NotImplementedException(); }
        }

        public virtual Create IStorageObject.Create
        {
            get { return (Create)AttachEvents(new Create(Uri)); }
        }

        public virtual Update IStorageObject.Update
        {
            get { return (Update)AttachEvents(new Update(Uri)); }
        }

        public virtual Delete IStorageObject.Delete
        {
            get { return (Delete)AttachEvents(new Delete(Uri)); } 
        }

        public virtual Get IStorageObject.Get
        {
            get { return (Get)AttachEvents(new Get(Uri)); }
        }

        protected virtual void Base_OnTimeout(Base sender, Client client, HttpConnection connection)
        {
            if (OnTimeout != null) OnTimeout(this, sender, client, connection);
            else throw new UnsupportedException("OnTimeout is not supported");
        }

        protected virtual void Base_OnProgress(Base sender, Client client, HttpConnection connection, DirectionType direction, int packetSize)
        {
            if (OnProgress != null) OnProgress(this, sender, client, connection, direction, packetSize);
        }

        protected virtual void Base_OnError(Base sender, Client client, string message, Exception exception)
        {
            if (OnError != null) OnError(this, sender, client, message, exception);
            else throw new UnsupportedException("OnError is not supported");
        }

        protected virtual void Base_OnComplete(Base sender, Client client, HttpConnection connection, HttpResponse response)
        {
            if (OnComplete != null) OnComplete(this, sender, client, connection, response);
            else throw new UnsupportedException("OnComplete is not supported");
        }
    }
}
