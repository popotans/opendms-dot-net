using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Work
{
    public class JobException
        : Exception
    {
        public JobException()
            : base()
        {
        }

        public JobException(string message)
            : base(message)
        {
        }

        public JobException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
