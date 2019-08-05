namespace Infrastructure
{
    public interface ISubscribe{
        void Subscribe<TMessage>(IHandle<TMessage> subscriber) where TMessage : IEvent;
        void Subscribe<TMessage>(IHandleCommand<TMessage> subscriber) where TMessage : ICommand;
    }
}
