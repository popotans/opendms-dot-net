using System;
using System.Collections.Generic;
using System.Text;

namespace Common.NetworkPackage
{
    public class ServerResponse : DictionaryBase<string, object>
    {
        public enum ErrorCode
        {
            None = 0,
            ExistingLease,
            ExistingResource,
            ReasourceIsLocked,
            InvalidGuid,
            InvalidRelativeVersion,
            InvalidReadOnlyValue,
            InvalidFormatting,
            InvalidPermissions,
            InvalidSearchParameters,
            FailedIndexing,
            ResourceDoesNotExist,
            Exception
        }

        public ServerResponse()
            : base("ServerResponse")
        {
        }

        public ServerResponse(bool pass, ErrorCode code)
            : base("ServerResponse")
        {
            Add("Pass", pass);
            Add("ErrorCode", code);
            Add("Timestamp", DateTime.Now);
        }

        public ServerResponse(bool pass, ErrorCode code, string message)
            : base("ServerResponse")
        {
            Add("Pass", pass);
            Add("ErrorCode", code);
            Add("Message", message);
            Add("Timestamp", DateTime.Now);
        }
    }
}
