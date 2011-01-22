using System;
using System.Collections.Generic;
using Apache.Cassandra;

namespace loHttpModule.Data
{
    public class MetaAsset
    {
        private System.Text.Encoding _utf8 = System.Text.Encoding.UTF8;
        private Cassandra.Client _client;
        private ColumnParent _columnParent;

        public MetaAsset(Cassandra.Client client)
        {
            if (client == null)
                throw new ArgumentNullException("client", "The argument 'client' cannot be null");

            _client = client;

            _columnParent = new ColumnParent();
            _columnParent.Column_family = "MetaAsset";
        }

        //public void SaveToCassandra(string keyspace, Common.NetworkPackage.MetaAsset ma)
        //{
        //    Dictionary<string, Dictionary<string, List<Mutation>>> package;
        //    Dictionary<string, List<Mutation>> packageEntry;
        //    Dictionary<string, object>.Enumerator en;
        //    List<Mutation> mutList;
        //    Mutation mut;
        //    DateTime now = DateTime.Now;

        //    if (!ma.ContainsKey("$guid")) throw new ArgumentException("The field $guid is not present");

        //    mutList = new List<Mutation>();
        //    en = (Dictionary<string, object>.Enumerator)ma.GetEnumerator();

        //    while (en.MoveNext())
        //    {
        //        if (en.Current.Value == null)
        //            continue;
        //        mut = new Mutation();
        //        mut.Column_or_supercolumn = new ColumnOrSuperColumn();
        //        mut.Column_or_supercolumn.Column = new Column();
        //        mut.Column_or_supercolumn.Column.Name = _utf8.GetBytes(en.Current.Key);

        //        if (en.Current.Value == null)
        //            mut.Column_or_supercolumn.Column.Value = null;
        //        else
        //            mut.Column_or_supercolumn.Column.Value = ToBytes(en.Current.Value);

        //        mut.Column_or_supercolumn.Column.Timestamp = Utilities.Timestamp(now);
        //        mutList.Add(mut);
        //    }

        //    packageEntry = new Dictionary<string, List<Mutation>>();
        //    packageEntry.Add("MetaAsset", mutList);

        //    package = new Dictionary<string, Dictionary<string, List<Mutation>>>();
        //    package.Add((string)ma["$guid"], packageEntry);

        //    //Console.WriteLine(Utilities.ExportBulkMutate("LawOffice", package));

        //    _client.batch_mutate(keyspace, package, ConsistencyLevel.ONE);
        //}

        //public Common.NetworkPackage.MetaAsset GetFromCassandra(string keyspace, Guid guid)
        //{
        //    string key;
        //    Common.NetworkPackage.MetaAsset ma;
        //    List<ColumnOrSuperColumn> results;
        //    SlicePredicate predicate = new SlicePredicate();
        //    predicate.Slice_range = new SliceRange();

        //    predicate.Slice_range.Start = _utf8.GetBytes("");
        //    predicate.Slice_range.Finish = _utf8.GetBytes("");
        //    predicate.Slice_range.Count = 100;
        //    predicate.Slice_range.Reversed = false;

        //    results = _client.get_slice(keyspace, guid.ToString("N"), _columnParent, predicate,
        //                                ConsistencyLevel.QUORUM);

        //    if (results.Count <= 0)
        //        return null;

        //    ma = new Common.NetworkPackage.MetaAsset();

        //    for (int i = 0; i < results.Count; i++)
        //    {
        //        key = _utf8.GetString(results[i].Column.Name);
        //        ma.Add(key, GetValue(results[i].Column.Value));
        //    }

        //    return ma;
        //}

        //private byte[] ToBytes(object value)
        //{
        //    Type type = value.GetType();
        //    string total = type.Name + ":";

        //    if (type == typeof(string))
        //        total += (string)value;
        //    else if (type == typeof(Int32))
        //        total += ((Int32)value).ToString();
        //    else if (type == typeof(UInt32))
        //        total += ((UInt32)value).ToString();
        //    else if (type == typeof(Int64))
        //        total += ((Int64)value).ToString();
        //    else if (type == typeof(UInt64))
        //        total += ((UInt64)value).ToString();
        //    else if (type == typeof(DateTime))
        //        total += Utilities.Timestamp((DateTime)value).ToString();
        //    else if (type == typeof(List<string>))
        //        total += ListToString((List<string>)value);
        //    else if (type == typeof(Common.SerializableStringList))
        //        total += ListToString((Common.SerializableStringList)value);
        //    else
        //        throw new InvalidCastException("Unsupported cast");

        //    return System.Text.Encoding.UTF8.GetBytes(total);
        //}

        private string ListToString(List<string> list)
        {
            string str = "";

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains(",")) throw new Exception("Lists cannot contain the ',' (comma) character");
                str += list[i];
                if (i + 1 < list.Count)
                    str += ",";
            }

            return str;
        }

        private object GetValue(byte[] bytes)
        {
            string type, val;

            val = _utf8.GetString(bytes);
            type = val.Substring(0, val.IndexOf(":"));
            val = val.Substring(val.IndexOf(":") + 1);

            switch (type)
            {
                case "String":
                    return val;
                case "Int32":
                    return int.Parse(val);
                case "UInt32":
                    return uint.Parse(val);
                case "Int64":
                    return long.Parse(val);
                case "UInt64":
                    return ulong.Parse(val);
                case "DateTime":
                    return Utilities.DateTimeFromTimestamp(long.Parse(val));
                case "List`1":
                    return StringToList(val);
                case "SerializableStringList":
                    return new Common.SerializableStringList().Import(StringToList(val));
                default:
                    throw new InvalidCastException("Unsupported cast");
            }
        }

        private List<string> StringToList(string str)
        {
            string[] a = str.Split(',');
            List<string> list = new List<string>(a.Length);
            for (int i = 0; i < a.Length; i++)
                list.Add(a[i]);
            return list;
        }
    }
}
