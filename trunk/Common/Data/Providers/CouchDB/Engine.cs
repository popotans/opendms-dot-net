namespace Common.Data.Providers.CouchDB
{
    public class Engine
    {
        public Engine()
        {
        }

        public void ExecuteCommand(Commands.Base cmd)
        {
            cmd.Execute();
        }
    }
}
