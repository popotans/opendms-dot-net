using System;

namespace OpenDMS.Storage.Security
{
    public class Session
    {
        public string UserId { get; private set; }
        public string Username { get; private set; }
        public Guid AuthToken { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime Expiry { get; private set; }

        public Session(string userid, Guid authToken, DateTime expiry)
        {
            UserId = userid;
            Username = userid.Substring(5);
            AuthToken = authToken;
            Start = DateTime.Now;
            Expiry = expiry;
        }
    }
}
