using System;
using EventStore.ClientAPI;

namespace Infrastructure
{
    public sealed class EventPump : IDisposable
    {
        private readonly IEventStoreConnection _conn;
        private readonly IBus _readerBus;
        private readonly Func<ResolvedEvent, object> _deserializer;
        private EventStoreAllCatchUpSubscription _subscription;
        public EventPump(IEventStoreConnection conn, IBus readerBus, Func<ResolvedEvent, Object> deserializer)
        {
            _conn = conn;
            _readerBus = readerBus;
            _deserializer = deserializer;
        }

        public void Start()
        {
            _subscription = _conn.SubscribeToAllFrom(
                Position.Start,
                CatchUpSubscriptionSettings.Default,
                (sub, evt) => {
                    if (evt.Event.Data.Length > 0 &&
                        evt.Event.IsJson && 
                        !evt.Event.EventType.StartsWith("$")) {
                        var e = _deserializer(evt);
                        if (e is IMessage message) {
                            _readerBus.Publish(message);
                        }
                    }
                }
                );
        }

        public void Dispose()
        {
            _subscription?.Stop();
        }
    }
}
