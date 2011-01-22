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
