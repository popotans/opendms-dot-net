using System;

namespace OpenDMS.HttpModule
{
    /// <summary>
    /// Represents a Service Point custom attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServicePointAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public enum VerbType
        {
            /// <summary>
            /// HTTP HEAD
            /// </summary>
            HEAD = 1,
            /// <summary>
            /// HTTP GET
            /// </summary>
            GET = 2,
            /// <summary>
            /// HTTP PUT
            /// </summary>
            PUT = 4,
            /// <summary>
            /// HTTP POST
            /// </summary>
            POST = 8,
            /// <summary>
            /// HTTP DELETE
            /// </summary>
            DELETE = 16,
            /// <summary>
            /// All verbs
            /// </summary>
            ALL = HEAD | GET | PUT | POST | DELETE,
        }

        /// <summary>
        /// Gets or sets the verb.
        /// </summary>
        /// <value>
        /// The verb.
        /// </value>
        public VerbType Verb { get; set; }
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePointAttribute"/> class.
        /// </summary>
        /// <param name="path">The service path.</param>
        public ServicePointAttribute(string path)
            : this(path, VerbType.ALL)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePointAttribute"/> class.
        /// </summary>
        /// <param name="path">The service path.</param>
        /// <param name="verb">The verb.</param>
        public ServicePointAttribute(string path, VerbType verb)
        {
            Path = path;
            Verb = verb;
        }

        /// <summary>
        /// Gets an integer value representing a weighted score for strength of match.  The higher the score, the more characters of the
        /// path that match.
        /// </summary>
        /// <param name="path">The service path.</param>
        /// <param name="verb">The verb.</param>
        /// <returns>An integer value representing a weighted score for strength of match.</returns>
        public int GetMatchRate(string path, string verb)
        {
            VerbType vt = StringToVerb(verb);

            if ((vt & Verb) == vt)
                return GetPathMatchRate(path);

            return -1;
        }

        /// <summary>
        /// Parses a <see cref="VerbType"/> from a string.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The parsed <see cref="VerbType"/>.</returns>
        private VerbType StringToVerb(string verb)
        {
            return (VerbType)Enum.Parse(typeof(VerbType), verb);
        }

        /// <summary>
        /// Gets the length of this instances Path property when the argument starts with this instance's Path
        /// </summary>
        /// <param name="path">The service path.</param>
        /// <returns>Length of this instances Path property.</returns>
        private int GetPathMatchRate(string path)
        {
            if (path.StartsWith(Path))
                return Path.Length;

            return -1;
        }
    }
}