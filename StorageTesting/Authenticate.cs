using System;
using OpenDMS.Storage.Providers;

namespace StorageTesting
{
    public class Authenticate : TestBase
    {
        public delegate void AuthenticationDelegate(OpenDMS.Storage.Security.Session session);
        public event AuthenticationDelegate OnAuthenticationSuccess;

        private DateTime _start;

        public Authenticate(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            FrmLogin win = new FrmLogin();
            win.OnCancelClick += new FrmLogin.CancelDelegate(win_OnCancelClick);
            win.OnLoginClick += new FrmLogin.LoginDelegate(win_OnLoginClick);
            win.ShowDialog();
        }

        void win_OnLoginClick(string username, string password)
        {            
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
            string encPassword = System.Convert.ToBase64String(sha512.ComputeHash(bytes));

            EngineRequest request = new EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.OnComplete = new EngineBase.CompletionDelegate(OnAuthenticated);
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.User;

            _engine.AuthenticateUser(request, username, encPassword);
        }

        void win_OnCancelClick()
        {
            WriteLine("Authenticate - Login cancelled.");
        }

        private void OnAuthenticated(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            Tuple<OpenDMS.Storage.Security.Session, bool> r = (Tuple<OpenDMS.Storage.Security.Session, bool>)result;

            if (!r.Item2)
            {
                WriteLine("Authenticate - The user failed authentication in " + duration.TotalMilliseconds.ToString() + "ms.");
                return;
            }

            WriteLine("Authenticate - the user passed authentication and has been assigned the authentication token: " + r.Item1.AuthToken.ToString());
            OnAuthenticationSuccess(r.Item1);
        }
    }
}
