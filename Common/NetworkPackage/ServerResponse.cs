/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
