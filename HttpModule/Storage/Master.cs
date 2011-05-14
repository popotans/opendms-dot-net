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
using Common;

namespace HttpModule.Storage
{
    public class Master
    {
        // Howto : make it pluggable http://weblogs.asp.net/justin_rogers/articles/61042.aspx

        /// <summary>
        /// A reference to <see cref="Common.FileSystem.IO"/> allowing file system access.
        /// </summary>
        private Common.FileSystem.IO _fileSystem;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Master"/> class.
        /// </summary>
        /// <param name="fileSystem">A reference to <see cref="Common.FileSystem.IO"/> allowing file system access.</param>
        public Master(Common.FileSystem.IO fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Common.Postgres.Version GetCurrentVersion(Guid resourceId)
        {
            Common.Postgres.Resource resource;

            // Instantiate the pgsql resource
            resource = new Common.Postgres.Resource(resourceId);
            
            // Gets the resource's current version
            return resource.GetCurrentVersion();
        }

        public Common.Postgres.Version GetVersionByNumber(Guid resourceId, UInt64 versionNumber)
        {
            Common.Postgres.Resource resource;

            // Instantiate the pgsql resource
            resource = new Common.Postgres.Resource(resourceId);

            // Gets the resource's current version
            return resource.GetVersion(versionNumber);
        }

        public Common.NetworkPackage.ServerResponse CheckoutResourceCurrentVersionByAnyVersion(Guid versionId, string requestingUser)
        {
            Common.Postgres.Resource pgResource;

            pgResource = Common.Postgres.Resource.GetResourceFromVersionId(versionId);

            return CheckoutResource(pgResource.Id, requestingUser);
        }

        public Common.NetworkPackage.ServerResponse CheckoutResource(Guid resourceId, string requestingUser)
        {
            Common.Postgres.Resource pgResource;
            Common.Postgres.Version pgVersion;

            // Get Resource from pgsql
            if ((pgResource = Common.Postgres.Resource.Get(resourceId)) == null)
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.ResourceDoesNotExist,
                    "The requested resource does not exist.");
            }

            // Not empty and locked by a different user
            if (!string.IsNullOrEmpty(pgResource.LockedBy) 
                && pgResource.LockedBy != requestingUser)
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.ResourceIsLocked,
                    "The requested resource is locked by another user.");
            }

            // Apply lock
            if (!pgResource.ApplyLock(requestingUser))
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.Exception,
                    "The resource could not be locked due to an unexpected exception.");
            }

            pgVersion = pgResource.GetCurrentVersion();

            // Give successful result
            return new Common.NetworkPackage.ServerResponse(true,
                Common.NetworkPackage.ServerResponse.ErrorCode.None,
                pgVersion.VersionNumber.ToString() + "-" + pgVersion.VersionGuid.ToString("N"));
        }

        public Common.NetworkPackage.ServerResponse CheckinResource(Guid resourceId, string requestingUser, bool isNew)
        {
            Common.Postgres.Resource pgResource;
            Common.Postgres.Version pgVersion;

            if (isNew)
            {
                pgResource = Common.Postgres.Resource.CreateNewResource(requestingUser, out pgVersion);
                return new Common.NetworkPackage.ServerResponse(true,
                    Common.NetworkPackage.ServerResponse.ErrorCode.None, 
                    pgVersion.VersionNumber.ToString() + "-" + pgVersion.VersionGuid.ToString("N"));
            }

            // Get the current Resource from pgsql
            if ((pgResource = Common.Postgres.Resource.Get(resourceId)) == null)
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.ResourceDoesNotExist,
                    "The requested resource does not exist.");
            }

            // Now we need to do some lock checking.
            // First, we need to check to see if a lock exists (not null)
            // Second, we need to ensure that the lock is owned by this user
            if (!string.IsNullOrEmpty(pgResource.LockedBy))
            {
                if (pgResource.LockedBy != requestingUser)
                {
                    return new Common.NetworkPackage.ServerResponse(false,
                        Common.NetworkPackage.ServerResponse.ErrorCode.ResourceIsLocked,
                        "The requested resource is locked by another user.");
                }
            }
            else
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.ResourceNotCheckedOut,
                    "The requested resource cannot be checked in because it has not been checked out.");
            }

            // Release lock
            if (!pgResource.ReleaseLock())
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.Exception,
                    "The lock on the resource could not be released due to an unexpected exception.");
            }

            // Go to next version
            pgVersion = pgResource.CreateNewVersion();

            // Give successful result
            return new Common.NetworkPackage.ServerResponse(true,
                Common.NetworkPackage.ServerResponse.ErrorCode.None,
                pgVersion.VersionNumber.ToString() + "-" + pgVersion.VersionGuid.ToString("N"));
        }

        public Common.NetworkPackage.ServerResponse ReleaseLock(Guid resourceId, string requestingUser)
        {
            Common.Postgres.Resource pgResource;

            // Get the current Resource from pgsql
            if ((pgResource = Common.Postgres.Resource.Get(resourceId)) == null)
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.ResourceDoesNotExist,
                    "The requested resource does not exist.");
            }

            // Now we need to do some lock checking.
            // First, we need to check to see if a lock exists (not null)
            // Second, we need to ensure that the lock is owned by this user
            if (!string.IsNullOrEmpty(pgResource.LockedBy))
            {
                if (pgResource.LockedBy != requestingUser)
                {
                    return new Common.NetworkPackage.ServerResponse(false,
                        Common.NetworkPackage.ServerResponse.ErrorCode.ResourceIsLocked,
                        "The requested resource is locked by another user.");
                }
            }
            else
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.ResourceNotCheckedOut,
                    "The requested resource cannot be unlocked because it has not been checked out.");
            }

            // Release lock
            if (!pgResource.ReleaseLock())
            {
                return new Common.NetworkPackage.ServerResponse(false,
                    Common.NetworkPackage.ServerResponse.ErrorCode.Exception,
                    "The lock on the resource could not be released due to an unexpected exception.");
            }

            // Give successful result
            return new Common.NetworkPackage.ServerResponse(true,
                Common.NetworkPackage.ServerResponse.ErrorCode.None);
        }

        /// <summary>
        /// Gets the meta form from the file system.
        /// </summary>
        /// <param name="requestingUser">The requesting user.</param>
        /// <param name="metaForm">The instantiated instance of the <see cref="Common.NetworkPackage.MetaForm"/>.</param>
        /// <param name="errorMessage">A string representing the cause of the error.</param>
        /// <returns>Success or Failure.</returns>
        public bool GetMetaForm(string requestingUser, out Common.NetworkPackage.MetaForm metaForm, out string errorMessage)
        {
            string relativeFilepath = "settings\\metapropertiesform.xml";

            errorMessage = null;
            metaForm = new Common.NetworkPackage.MetaForm();

            // TODO : setup user checking

            try
            {
                if (!metaForm.ReadFromFile(relativeFilepath, _fileSystem))
                {
                    errorMessage = "Unable to read from the saved search form.";
                    return false;
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                errorMessage = "Unable to read from the saved search form.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the search form from the file system.
        /// </summary>
        /// <param name="requestingUser">The requesting user.</param>
        /// <param name="searchForm">The instantiated instance of the <see cref="Common.NetworkPackage.SearchForm"/>.</param>
        /// <param name="errorMessage">A string representing the cause of the error.</param>
        /// <returns>Success or Failure.</returns>
        public bool GetSearchForm(string requestingUser, out Common.NetworkPackage.SearchForm searchForm, out string errorMessage)
        {
            string relativeFilepath = "settings\\searchform.xml";

            errorMessage = null;
            searchForm = new Common.NetworkPackage.SearchForm();

            // TODO : setup user checking

            if (!searchForm.ReadFromFile(relativeFilepath, _fileSystem))
            {
                errorMessage = "Unable to read from the saved search form.";
                return false;
            }

            return true;
        }
    }
}
