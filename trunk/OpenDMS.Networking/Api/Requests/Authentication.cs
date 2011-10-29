using System;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api.Requests
{
    public class Authentication : RequestBase
    {
        public string Username
        {
            get
            {
                if (ApplicationContent["Username"] == null)
                    throw new MessageFormattingException("No username specified.");
                return ApplicationContent["Username"].Value<string>();
            }
            set
            {
                if (ApplicationContent["Username"] != null)
                    ApplicationContent["Username"] = value;
                else
                    ApplicationContent.Add("Username", value);
            }
        }

        public string HashedPassword
        {
            get
            {
                if (ApplicationContent["Password"] == null)
                    throw new MessageFormattingException("No password specified.");
                return ApplicationContent["Password"].Value<string>();
            }
            set
            {
                if (ApplicationContent["Password"] != null)
                    ApplicationContent["Password"] = value;
                else
                    ApplicationContent.Add("Password", value);
            }
        }

        public string ClearTextPassword
        {
            set
            {
                System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
                HashedPassword = System.Convert.ToBase64String(sha512.ComputeHash(bytes));
            }
        }

        public Authentication()
        {
        }

        public Authentication(JObject fullContent)
        {
            FullContent = fullContent;
        }

        public override Http.Methods.Request CreateRequest(Uri uri, string contentType, long contentLength)
        {
            return new Http.Methods.Post(uri, contentType, (ulong)contentLength);
        }
    }
}
