using System;

namespace OpenDMS
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServicePointAttribute : Attribute
    {
        public enum VerbType
        {
            HEAD = 1,
            GET = 2,
            PUT = 4,
            POST = 8,
            DELETE = 16,
            ALL = HEAD | GET | PUT | POST | DELETE,
        }

        public VerbType Verb { get; set; }
        public string Path { get; set; }

        public ServicePointAttribute(string path) 
            : this(path, VerbType.ALL)
        {
        }

        public ServicePointAttribute(string path, VerbType verb)
        {
            Path = path;
            Verb = verb;
        }

        /// <summary>
        /// Gets an integer value representing a weighted score for strength of match.  The higher the score, the more characters of the 
        /// path that match.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="verb"></param>
        /// <returns></returns>
        public int GetMatchRate(string path, string verb)
        {
            VerbType vt = StringToVerb(verb);

            if ((vt & Verb) == vt)
                return GetPathMatchRate(path);

            return -1;
        }

        private VerbType StringToVerb(string verb)
        {
            return (VerbType)Enum.Parse(typeof(VerbType), verb);
        }

        /// <summary>
        /// Gets the length of this instances Path property when the argument starts with this instance's Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private int GetPathMatchRate(string path)
        {
            if (path.StartsWith(Path))
                return Path.Length;

            return -1;
        }
    }
}
