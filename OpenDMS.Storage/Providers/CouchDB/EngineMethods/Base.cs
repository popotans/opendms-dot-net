
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public abstract class Base
    {
        protected bool _isEventSubscriptionSuppressed = false;
        protected EngineRequest _request;
        
        protected Engine.ActionDelegate _onActionChanged = null;
        protected Engine.ProgressDelegate _onProgress = null;
        protected Engine.CompletionDelegate _onComplete = null;
        protected Engine.TimeoutDelegate _onTimeout = null;
        protected Engine.ErrorDelegate _onError = null;

        public abstract void Execute();

        public Base(EngineRequest request)
        {
            if (request == null) return;

            _request = request;
            _onActionChanged = request.OnActionChanged;
            _onProgress = request.OnProgress;
            _onComplete = request.OnComplete;
            _onTimeout = request.OnTimeout;
            _onError = request.OnError;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.TimeoutDelegate onTimeout)
        {
            Commands.Base.TimeoutDelegate timeoutDelegate = (sender, client, connection) =>
            {
                try
                {
                    if (onTimeout != null &&
                        !_isEventSubscriptionSuppressed)
                        onTimeout(_request);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onTimeout argument.", e);
                    throw;
                }
            };
            cmd.OnTimeout += timeoutDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.ErrorDelegate onError)
        {
            Commands.Base.ErrorDelegate errorDelegate = (sender, client, message, exception) =>
            {
                try
                {
                    if (onError != null &&
                        !_isEventSubscriptionSuppressed)
                        onError(_request, message, exception);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
                    throw;
                }
            };
            cmd.OnError += errorDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.CompletionDelegate onComplete)
        {
            Commands.Base.CompletionDelegate completionDelegate = (sender, client, connection, reply) =>
            {
                try
                {
                    if (onComplete != null &&
                        !_isEventSubscriptionSuppressed)
                        onComplete(_request, reply);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onComplete argument.", e);
                    throw;
                }
            };
            cmd.OnComplete += completionDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.ProgressDelegate onProgress)
        {
            Commands.Base.ProgressDelegate progressDelegate = (sender, client, connection, direction, packetSize, sendPercentComplete, receivePercentComplete) =>
            {
                try
                {
                    if (onProgress != null &&
                        !_isEventSubscriptionSuppressed)
                        onProgress(_request, direction, packetSize, sendPercentComplete, receivePercentComplete);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
                    throw;
                }
            };
            cmd.OnProgress += progressDelegate;
        }
    }
}
