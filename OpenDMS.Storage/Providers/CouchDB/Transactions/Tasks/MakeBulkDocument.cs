using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class MakeBulkDocument : Base
    {
        private List<Model.Document> _docs;

        public Model.BulkDocuments BulkDocument { get; private set; }

        public MakeBulkDocument(List<Model.Document> doc,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _docs = doc;
        }

        public override void Process()
        {
            BulkDocument = new Model.BulkDocuments();
            for (int i = 0; i < _docs.Count; i++)
                BulkDocument.AddDocument(_docs[i]);

            TriggerOnComplete(null);
        }
    }
}
