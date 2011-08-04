using System;

namespace OpenDMS.Storage.Security
{
    public class Session
    {
        public User User { get; private set; }
        public Guid AuthToken { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime Expiry { get; private set; }
        public bool IsSecurityOverride { get; private set; }

        public Session(User user, Guid authToken, DateTime expiry)
        {
            User = user;
            AuthToken = authToken;
            Start = DateTime.Now;
            Expiry = expiry;
        }

        public static Session MakeSecurityOverride()
        {
            Session s = new Session(new Security.User("System", null, "system", null, null, null, null, true), 
                Guid.Empty, DateTime.Now.AddMinutes(15));
            s.IsSecurityOverride = true;
            return s;
        }
    }
}
