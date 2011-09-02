using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class FormattingException : Exception
    {
        public FormattingException()
            : base()
        {
        }

        public FormattingException(string message)
            : base(message)
        {
        }

        public FormattingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
