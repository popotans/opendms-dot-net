using System;

namespace Common.Data
{
    /// <summary>
    /// Provides a data representation of the header information returned from the server as the result 
    /// of a HTTP HEAD request.
    /// </summary>
    public class Head
    {
        /// <summary>
        /// Gets or sets the <see cref="ETag"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ETag"/>.
        /// </value>
        public ETag ETag { get; set; }
        /// <summary>
        /// Gets or sets a string representation of the MD5.
        /// </summary>
        /// <value>
        /// The MD5 value.
        /// </value>
        public string MD5 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Head"/> class.
        /// </summary>
        /// <param name="etag">The <see cref="ETag"/>.</param>
        /// <param name="md5">The MD5.</param>
        public Head(ETag etag, string md5)
        {
            ETag = etag;
            MD5 = md5;
        }
    }
}
