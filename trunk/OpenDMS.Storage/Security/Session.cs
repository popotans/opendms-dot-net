using System;

namespace OpenDMS.Storage.Security
{
    public class Session
    {
        public User User { get; private set; }
        public Guid AuthToken { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime Expiry { get; private set; }

        public Session(User user, Guid authToken, DateTime expiry)
        {
            User = user;
            AuthToken = authToken;
            Start = DateTime.Now;
            Expiry = expiry;
        }
    }
}
