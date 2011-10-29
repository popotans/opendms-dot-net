using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api.Responses
{
    public class Authentication : ResponseBase
    {
        public bool Success
        {
            get
            {
                if (ApplicationContent["Success"] == null)
                    throw new MessageFormattingException("No Success specified.");
                return ApplicationContent["Success"].Value<bool>();
            }
            set
            {
                if (ApplicationContent["Success"] != null)
                    ApplicationContent["Success"] = value;
                else
                    ApplicationContent.Add("Success", value);
            }
        }

        public Guid AuthToken
        {
            get
            {
                if (ApplicationContent["AuthToken"] == null)
                    throw new MessageFormattingException("No AuthToken specified.");
                return Guid.Parse(ApplicationContent["AuthToken"].Value<string>());
            }
            set
            {
                if (ApplicationContent["AuthToken"] != null)
                    ApplicationContent["AuthToken"] = value.ToString();
                else
                    ApplicationContent.Add("AuthToken", value.ToString());
            }
        }

        public DateTime Expiry
        {
            get
            {
                if (ApplicationContent["Expiry"] == null)
                    throw new MessageFormattingException("No Expiry specified.");
                return ApplicationContent["Expiry"].Value<DateTime>();
            }
            set
            {
                if (ApplicationContent["Expiry"] != null)
                    ApplicationContent["Expiry"] = value;
                else
                    ApplicationContent.Add("Expiry", value);
            }
        }

        public static new ResponseBase BuildFrom(Requests.RequestBase request)
        {
            throw new InvalidOperationException("Use BuildFrom(Requests.RequestBase, object)");
        }

        public static new ResponseBase BuildFrom(Requests.RequestBase request, params object[] obj)
        {
            Authentication auth = new Authentication();
            if (obj.Length == 3)
            {
                if (obj[0].GetType() != typeof(bool))
                    throw new ArgumentException("Argument obj[0] must be of type bool.");
                if (obj[1].GetType() != typeof(Guid))
                    throw new ArgumentException("Argument obj[1] must be of type Guid.");
                if (obj[2].GetType() != typeof(DateTime))
                    throw new ArgumentException("Argument obj[2] must be of type DateTime.");

                auth.Success = (bool)obj[0];
                auth.AuthToken = (Guid)obj[1];
                auth.Expiry = (DateTime)obj[2];
            }
            else if (obj.Length == 1)
            {
                if (obj[0].GetType() != typeof(bool))
                    throw new ArgumentException("Argument obj[0] must be of type bool.");

                auth.Success = (bool)obj[0];
            }
            else
                throw new ArgumentException("Argument obj is expected to have either 1 or 3 elements.");

            return auth;
        }
    }
}
