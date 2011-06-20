
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public abstract class Base
    {
        protected bool _isEventSubscriptionSuppressed = false;
        
        protected Engine.ActionDelegate _onActionChanged = null;
        protected Engine.ProgressDelegate _onProgress = null;
        protected Engine.CompletionDelegate _onComplete = null;
        protected Engine.TimeoutDelegate _onTimeout = null;
        protected Engine.ErrorDelegate _onError = null;

        public abstract void Execute();

        public Base(Engine.ActionDelegate onActionChanged,
            Engine.ProgressDelegate onProgress,
            Engine.CompletionDelegate onComplete,
            Engine.TimeoutDelegate onTimeout,
            Engine.ErrorDelegate onError)
        {
            _onActionChanged = onActionChanged;
            _onProgress = onProgress;
            _onComplete = onComplete;
            _onTimeout = onTimeout;
            _onError = onError;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.TimeoutDelegate onTimeout)
        {
            Commands.Base.TimeoutDelegate timeoutDelegate = (sender, client, connection) =>
            {
                if (onTimeout != null && 
                    !_isEventSubscriptionSuppressed) 
                    onTimeout();
            };
            cmd.OnTimeout += timeoutDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.ErrorDelegate onError)
        {
            Commands.Base.ErrorDelegate errorDelegate = (sender, client, message, exception) =>
            {
                if (onError != null &&
                    !_isEventSubscriptionSuppressed) 
                    onError(message, exception);
            };
            cmd.OnError += errorDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.CompletionDelegate onComplete)
        {
            Commands.Base.CompletionDelegate completionDelegate = (sender, client, connection, reply) =>
            {
                if (onComplete != null &&
                    !_isEventSubscriptionSuppressed) 
                    onComplete(reply);
            };
            cmd.OnComplete += completionDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.ProgressDelegate onProgress)
        {
            Commands.Base.ProgressDelegate progressDelegate = (sender, client, connection, direction, packetSize, sendPercentComplete, receivePercentComplete) =>
            {
                if (onProgress != null &&
                    !_isEventSubscriptionSuppressed) 
                    onProgress(direction, packetSize, sendPercentComplete, receivePercentComplete);
            };
            cmd.OnProgress += progressDelegate;
        }
    }
}
