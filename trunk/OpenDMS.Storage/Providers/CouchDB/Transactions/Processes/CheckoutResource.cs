using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class CheckoutResource : Base
    {
        private IDatabase _db;
        private Data.ResourceId _id;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;

        public Data.Resource Resource { get; private set; }
        public JObject Remainder { get; private set; }

        public CheckoutResource(IDatabase db, Data.ResourceId id, 
            Security.RequestingPartyType requestingPartyType, Security.Session session)
            : base()
        {
            _db = db;
            _id = id;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadResource(_db, _id));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadResource))
            {
                Tasks.DownloadResource task = (Tasks.DownloadResource)sender;
                Resource = task.Resource;
                Remainder = task.Remainder;
                RunTaskProcess(new Tasks.CheckResourcePermissions(_db, Resource, _requestingPartyType,
                    _session, Security.Authorization.ResourcePermissionType.Checkout));
            }
            else if (t == typeof(Tasks.CheckResourcePermissions))
            {
                Tasks.CheckResourcePermissions task = (Tasks.CheckResourcePermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }
                RunTaskProcess(new Tasks.MarkResourceForCheckout(Resource, _session.User.Username));
            }
            else if (t == typeof(Tasks.MarkResourceForCheckout))
            {
                Tasks.MarkResourceForCheckout task = (Tasks.MarkResourceForCheckout)sender;
                Resource = task.Resource;
                RunTaskProcess(new Tasks.UploadResource(_db, Resource));
            }
            else if (t == typeof(Tasks.UploadResource))
            {
                TriggerOnComplete(reply, new Tuple<Data.Resource, JObject>(Resource, Remainder));
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
