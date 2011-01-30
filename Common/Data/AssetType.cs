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

namespace Common.Data
{
    public class AssetType
    {
        public enum Type
        {
            Unknown = 0,
            Meta,
            Data
        }

        public static AssetType Meta = new AssetType(Type.Meta);
        public static AssetType Data = new AssetType(Type.Data);

        private Type _type;

        public Type Value
        {
            get { return _type; }
            set { _type = value; }
        }

        public string VirtualPath
        {
            get
            {
                switch (_type)
                {
                    case Type.Meta:
                        return "meta";
                    case Type.Data:
                        return "data";
                    default:
                        throw new ArgumentException("Invalid asset type.");
                }
            }
        }

        public AssetType(Type type)
        {
            _type = type;
        }

        public static bool IsNullOrUnknown(AssetType assetType)
        {
            if (assetType == null) return true;
            else if (assetType.Value == Type.Unknown) return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(AssetType))
                return ((AssetType)obj).Value == _type;
            else
                throw new ArgumentException("Invalid argument type.");
        }

        public override int GetHashCode()
        {
            return _type.GetHashCode();
        }

        public string GetExtension()
        {
            switch (_type)
            {
                case Type.Meta:
                    return ".xml";
                case Type.Data:
                    throw new Exception("An AssetType.Data's extension cannot be determined by this class.");
                default:
                    throw new Exception("Unknown AssetType.");
            }
        }
    }
}
