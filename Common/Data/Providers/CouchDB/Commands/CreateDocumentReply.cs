using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class CreateDocumentReply : ReplyBase
    {
        private const string _201 = "Document has been created successfully.";
        private const string _202 = "Document accepted for writing (batch mode).";
        private const string _409 = "Conflict - a document with the specified document ID already exists.";

        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }

        public CreateDocumentReply(Response response)
            : base(response)
        {
            
        }

        protected override void ParseResponse()
        {
            throw new System.NotImplementedException();
        }
    }
}
