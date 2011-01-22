using System;

namespace Common.Data
{
    public class ETag
    {
        private string _value;
        public string Value { get { return _value; } }

        public ETag(string value)
        {
            _value = value;
        }

        public bool IsNewer(ETag compare)
        {
            UInt64 a, b;

            if (compare == null)
                return true;

            try
            {
                a = UInt64.Parse(_value);
                b = UInt64.Parse(compare.Value);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Value must be castable to UInt64.");
            }

            return a > b;
        }

        public bool IsOlder(ETag compare)
        {
            UInt64 a, b;

            if (compare == null)
                return false;

            try
            {
                a = UInt64.Parse(_value);
                b = UInt64.Parse(compare.Value);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Value must be castable to UInt64.");
            }

            return a < b;
        }

        //public static bool operator ==(ETag etag1, ETag etag2)
        //{
        //    if (etag1 is null || etag2 is null)
        //        return false;
        //    return etag1.Value == etag2.Value;
        //}

        //public static bool operator !=(ETag etag1, ETag etag2)
        //{
        //    if (etag1 == null || etag2 == null)
        //        return true;
        //    return etag1.Value != etag2.Value;
        //}

        //public static bool operator <(ETag etag1, ETag etag2)
        //{
        //    if (etag1 == null || etag2 == null)
        //        throw new ArgumentNullException("One of the arguments was null");
        //    return etag1.IsNewer(etag2);
        //}

        //public static bool operator >(ETag etag1, ETag etag2)
        //{
        //    if (etag1 == null || etag2 == null)
        //        throw new ArgumentNullException("One of the arguments was null");
        //    return etag1.IsOlder(etag2);
        //}

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ETag))
                return _value == ((ETag)obj).Value;

            throw new Exception("Invalid argument type");
        }

        public ETag Increment()
        {
            UInt64 value = UInt64.Parse(_value);
            value++;
            _value = value.ToString();
            return this;
        }

        public ETag Increment(int value)
        {
            UInt64 temp = UInt64.Parse(_value);
            temp = temp + (UInt64)value;
            _value = temp.ToString();
            return this;
        }

        public static ETag operator +(ETag etag, int value)
        {
            UInt64 val = UInt64.Parse(etag.Value) + (UInt64)value;
            return new ETag(val.ToString());
        }
		
		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

        public override string ToString()
        {
            return _value;
        }

        public NetworkPackage.ETag ExportToNetworkRepresentation()
        {
            if (string.IsNullOrEmpty(Value))
                throw new Exception("Value cannot be empty");

            NetworkPackage.ETag etag = new NetworkPackage.ETag();

            etag.Add("Value", this.Value);

            return etag;
        }
    }
}
