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
using Common.FileSystem;

namespace HttpModule.Transactions
{
    public class Coordinator
    {
        private IO _fileSystem;
        private Storage.Master _storage;

        public Coordinator(IO fileSystem, Storage.Master storage)
        {
            _fileSystem = fileSystem;
            _storage = storage;
        }

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="guid">The GUID of the asset.</param>
        /// <param name="requestingUser">The requesting user.</param>
        /// <param name="durationOfExpiration">The quantity of milliseconds to run before expiration.</param>
        /// <param name="errorMessage">An error message.</param>
        /// <returns>The reference to the created <see cref="Transaction"/></returns>
        public Transaction Begin(Guid guid, string requestingUser, ulong durationOfExpiration, 
            out string errorMessage)
        {
            Transaction t;
            Common.Data.FullAsset fa;
            Common.Data.MetaAsset ma;
            Storage.ResultType result;
            string tranPath;
            
            // base transaction path for this request
            tranPath = @"transactions/" + guid.ToString("N") + "//";

            // Check lock, Apply lock
            result = _storage.LoadMeta(guid, true, true, requestingUser, false, out ma, out errorMessage);

            if(result != Storage.ResultType.Success)
                return null;

            // Directory exists, fail out
            if (_fileSystem.DirectoryExists(tranPath))
            {
                errorMessage = "A transaction is already operating on this resource.";
                return null;
            }

            t = new Transaction(_fileSystem);

            fa = new Common.Data.FullAsset(ma, new Common.Data.DataAsset(ma, _fileSystem));
            if (!t.Begin(fa, tranPath, requestingUser, durationOfExpiration))
                return null;

            return t;
        }

        public Transaction NextStep(Guid guid, string requestingUser, out string errorMessage)
        {
            Transaction t;
            string tranPath;

            // base transaction path for this request
            tranPath = @"transactions/" + guid.ToString("N") + "//";

            t = new Transaction(_fileSystem);
            if (!t.NextStep(tranPath, requestingUser))
            {
                errorMessage = "The transaction could not be advanced to the next step.";
                return null;
            }

            errorMessage = null;
            return t;
        }

        public Transaction Undo(Guid guid, string requestingUser, int steps, out string errorMessage)
        {
            Transaction t;
            string tranPath;

            // base transaction path for this request
            tranPath = @"transactions/" + guid.ToString("N") + "//";

            t = new Transaction(_fileSystem);
            if (!t.Undo(tranPath, requestingUser, steps))
            {
                errorMessage = "The action within the transaction could not be undone.";
                return null;
            }

            errorMessage = null;
            return t;
        }

        public Transaction Abort(Guid guid, string requestingUser, out string errorMessage)
        {
            Transaction t;
            string tranPath;

            // base transaction path for this request
            tranPath = @"transactions/" + guid.ToString("N") + "//";

            t = new Transaction(_fileSystem);
            if (!t.Abort(tranPath, requestingUser))
            {
                errorMessage = "The transaction could not be aborted.";
                return null;
            }

            errorMessage = null;
            return t;
        }

        public Transaction Commit(Guid guid, string requestingUser, out string errorMessage)
        {
            Transaction t;
            string tranPath;

            // base transaction path for this request
            tranPath = @"transactions/" + guid.ToString("N") + "//";

            t = new Transaction(_fileSystem);
            if (!t.Commit(guid, tranPath, requestingUser))
            {
                errorMessage = "The transaction could not be committed.";
                return null;
            }

            errorMessage = null;
            return t;
        }
    }
}