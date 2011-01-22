using System;
using System.IO;
using System.Xml;
using System.Text;

namespace Common.NetworkPackage
{
    public class MetaAsset : DictionaryBase<string, object>
    {
        public MetaAsset() 
            : base("MetaAsset")
        {
        }

        public bool Validate()
        {
            if (!ContainsKey("$guid")) return false;
            if (!ContainsKey("$etag")) return false;
            if (!ContainsKey("$metaversion")) return false;
            if (!ContainsKey("$dataversion")) return false;
            if (!ContainsKey("$creator")) return false;
            if (!ContainsKey("$length")) return false;
            if (!ContainsKey("$md5")) return false;
            if (!ContainsKey("$extension")) return false;
            if (!ContainsKey("$created")) return false;
            if (!ContainsKey("$modified")) return false;
            if (!ContainsKey("$lastaccess")) return false;
            if (!ContainsKey("$title")) return false;
            if (!ContainsKey("$tags")) return false;

            if (this["$guid"].GetType() != typeof(Guid)) return false;
            if (this["$etag"].GetType() != typeof(string)) return false;
            if (this["$metaversion"].GetType() != typeof(uint)) return false;
            if (this["$dataversion"].GetType() != typeof(uint)) return false;
            if (ContainsKey("$lockedby")) { if (this["$lockedby"].GetType() != typeof(string)) return false; }
            if (ContainsKey("$lockedat")) { if (this["$lockedat"].GetType() != typeof(DateTime?)) return false; }
            if (this["$creator"].GetType() != typeof(string)) return false;
            if (this["$length"].GetType() != typeof(ulong)) return false;
            if (this["$md5"].GetType() != typeof(string)) return false;
            if (this["$extension"].GetType() != typeof(string)) return false;
            if (this["$created"].GetType() != typeof(DateTime)) return false;
            if (this["$modified"].GetType() != typeof(DateTime)) return false;
            if (this["$lastaccess"].GetType() != typeof(DateTime)) return false;
            if (this["$title"].GetType() != typeof(string)) return false;
            if (this["$tags"].GetType() != typeof(System.Collections.Generic.List<string>)) return false;

            return true;
        }
    }
}
