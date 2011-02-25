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
using Common.Data;
using Common.FileSystem;

namespace HttpModule.Transactions
{
    public class Transaction
    {
        private IO _fileSystem;

        public Transaction(IO fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool Begin(FullAsset fa, string relativeTransactionPath, string requestingUser, 
            ulong durationOfExpiration)
        {
            Lock l;

            // Create the initial step directory
            if (!_fileSystem.DirectoryExists(relativeTransactionPath + "0"))
                _fileSystem.CreateDirectoryPath(relativeTransactionPath + "0");
            else
            {
                if (_fileSystem.ResourceExists(relativeTransactionPath + "lock.xml"))
                {
                    // We do not need to check if it was the same user because we do not care,
                    // if the transaction is open, it cannot begin, thus error
                    return false;
                }
            }

            // Create the lock
            l = new Lock(requestingUser, DateTime.Now, DateTime.Now.AddMilliseconds(durationOfExpiration), 
                durationOfExpiration);

            // Write it to the FS
            l.SaveToFile(relativeTransactionPath + "lock.xml", _fileSystem, true);

            // Copy resource to 0 step directory
            if (!fa.MetaAsset.Resource.CopyToRelativeFilePath(relativeTransactionPath + "0//meta.xml"))
                return false;

            if (!fa.DataAsset.Resource.CopyToRelativeFilePath(relativeTransactionPath + "0//data." + fa.MetaAsset.Extension))
                return false;

            return true;
        }

        public bool Commit(Guid guid, string relativeTransactionPath, string requestingUser)
        {
            string temp;
            MetaAsset ma;
            DataAsset da;
            Version version;
            Lock l = new Lock();
            int currentStep;

            // Read the lock
            if (!l.ReadFromFile(relativeTransactionPath + "lock.xml", _fileSystem))
                return false;

            // Check if the lock matches the requesting user
            if (l.LockedBy != requestingUser)
                return false;

            // Get the current step
            currentStep = GetCurrentStep(relativeTransactionPath);


            // Load MetaAsset
            ma = new MetaAsset(guid, _fileSystem);

            // Does it exist?
            if (!ma.ResourceExistsOnFilesystem())
                return false;

            // Load the MetaAsset
            if (!ma.Load())
                return false;

            // Load the DataAsset
            da = new DataAsset(ma, _fileSystem);

            // Create the Version
            version = new Version(new FullAsset(ma, da));

            // Copy the current version's metaasset into history
            if (!version.CopyMetaUsingVersionScheme())
                return false;

            // Copy the current version's dataasset into history
            if (!version.CopyDataUsingVersionScheme())
            {
                temp = ma.Resource.RelativeDirectory + ma.GuidString + "_" + 
                    version.ToString() + ma.Extension;

                // Delete the DA from history if it was created
                if (_fileSystem.ResourceExists(temp))
                    _fileSystem.Delete(temp);

                temp = ma.Resource.RelativeDirectory + ma.GuidString + "_" +
                    version.ToString() + ".xml";

                // Delete the MA from history if it was created
                if (_fileSystem.ResourceExists(temp))
                    _fileSystem.Delete(temp);

                return false;
            }

            // If we have made it here, both MA and DA were stored into history

            // Increment version
            version.IncrementMetaVersion();
            version.IncrementDataVersion();

            // Update the MA
            ma.UpdateByServer(version.MetaAsset.ETag, version.MetaAsset.MetaVersion, version.MetaAsset.DataVersion,
                requestingUser, DateTime.Now, ma.Creator, ma.Length, da.Resource.ComputeMd5(),
                ma.Created, ma.Modified, ma.LastAccess);

            // Save the MA
            ma.Save();

            return true;
        }

        public bool Abort(string relativeTransactionPath, string requestingUser)
        {
            Lock l = new Lock();

            // Read the lock
            if (!l.ReadFromFile(relativeTransactionPath + "lock.xml", _fileSystem))
                return false;

            // Check if the lock matches the requesting user
            if (l.LockedBy != requestingUser)
                return false;

            // Delete all content within the transaction path
            return _fileSystem.DeleteDirectoryAndAllContents(relativeTransactionPath);
        }

        public bool Undo(string relativeTransactionPath, string requestingUser, int steps)
        {
            Lock l = new Lock();
            int currentStep, deleteStep;

            // Read the lock
            if (!l.ReadFromFile(relativeTransactionPath + "lock.xml", _fileSystem))
                return false;

            // Check if the lock matches the requesting user
            if (l.LockedBy != requestingUser)
                return false;

            // Get the current step
            currentStep = GetCurrentStep(relativeTransactionPath);

            // Set the step to delete
            deleteStep = currentStep;

            while (steps > 0)
            {
                // Delete the deleteStep directory and all contents
                if (!_fileSystem.DeleteDirectoryAndAllContents(relativeTransactionPath +
                    deleteStep.ToString()))
                    return false;

                // Revert the step
                deleteStep--;

                // Decrement the amount of steps to revert
                steps--;
            }

            // Update the lock
            l.Expiry = l.Expiry.Value.AddMilliseconds(l.Duration.Value);
            l.SaveToFile(relativeTransactionPath + "lock.xml", _fileSystem, true);

            return true;
        }

        public bool NextStep(string relativeTransactionPath, string requestingUser)
        {
            Lock l = new Lock();
            int currentStep, nextStep;
            string extension;

            // Read the lock
            if (!l.ReadFromFile(relativeTransactionPath + "lock.xml", _fileSystem))
                return false;

            // Check if the lock matches the requesting user
            if (l.LockedBy != requestingUser)
                return false;

            // Get the current step
            currentStep = GetCurrentStep(relativeTransactionPath);

            // Advance to next step
            nextStep = currentStep + 1;

            // Create new step directory
            _fileSystem.CreateDirectoryPath(relativeTransactionPath + nextStep.ToString());

            // Get the extension of the data file
            extension = GetDataExtension(relativeTransactionPath + currentStep.ToString());

            // Copy current to new step directory
            if (!_fileSystem.Copy(relativeTransactionPath + currentStep.ToString() + "//meta.xml",
                relativeTransactionPath + nextStep.ToString() + "//meta.xml"))
                return false;
            if (!_fileSystem.Copy(relativeTransactionPath + currentStep.ToString() + "//data" + extension,
                relativeTransactionPath + nextStep.ToString() + "//meta" + extension))
                return false;

            // Update the lock
            l.Expiry = l.Expiry.Value.AddMilliseconds(l.Duration.Value);
            l.SaveToFile(relativeTransactionPath + "lock.xml", _fileSystem, true);

            return true;
        }

        /// <summary>
        /// Gets the current step.
        /// </summary>
        /// <param name="relativeTransactionPath">The relative transaction path.</param>
        /// <returns>An integer value of the current step.</returns>
        private int GetCurrentStep(string relativeTransactionPath)
        {
            System.IO.DirectoryInfo[] dirs;
            int step = 0, temp;

            // Gets a listing of the directories within the relative transaction path
            dirs = _fileSystem.GetDirectoryInfo(relativeTransactionPath).GetDirectories();

            for (int i = 0; i < dirs.Length; i++)
            {
                temp = int.Parse(dirs[i].Name);
                if (temp > step) step = temp;
            }

            return step;
        }

        /// <summary>
        /// Gets the extension of the data file within a directory where a [something].xml and a 
        /// [something].[ext] exist, make sure there are not multiple non-.xml files because this 
        /// method returns the first non-.xml file found.
        /// </summary>
        /// <param name="relativeStepPath">The relative path for the step.</param>
        /// <returns>A string representation of the extension of the file.</returns>
        private string GetDataExtension(string relativeStepPath)
        {
            string[] files = _fileSystem.GetFiles(relativeStepPath);

            for(int i=0; i<files.Length; i++)
            {
                if(System.IO.Path.GetExtension(files[i]) != ".xml")
                    return System.IO.Path.GetExtension(files[i]);
            }

            return null;
        }
    }
}