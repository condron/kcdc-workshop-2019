using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Infrastructure
{
    public abstract class Reader<TModel> :
        EventDrivenStateMachine,
        IDisposable,
        IReadModel<TModel> where TModel : class, new()
        
    {
        private readonly object _updateLock = new object();
        private readonly Func<IEventStoreConnection> _getConn;
        private readonly Func<ResolvedEvent, object> _deserializer;
        private EventStoreAllCatchUpSubscription _subscription;
        private bool _isLive;

        protected TModel Model;
        private Position _checkPoint;

        private readonly List<Action<TModel>> _subscribers = new List<Action<TModel>>();

        protected Reader(
            Func<IEventStoreConnection> conn,
            Func<ResolvedEvent, object> deserializer,
            SnapShot<TModel> snapshot = null)
        {
            _getConn = conn;
            _deserializer = deserializer;
            _checkPoint = snapshot?.At ?? Position.Start;
            Model = snapshot?.Data ?? new TModel();
        }
        
        public void Start()
        {
            _subscription = _getConn().SubscribeToAllFrom(
                Position.Start,
                CatchUpSubscriptionSettings.Default,
                GotEvent,
                (_) => { _isLive = true; }
            );
            SpinWait.SpinUntil(() => _isLive);
            UpdateSubscribers();
        }

        public void Stop()
        {
            _subscription?.Stop();
        }

        private Task GotEvent(EventStoreCatchUpSubscription sub, ResolvedEvent evt)
        {
            lock (_updateLock) {
                if (evt.Event.Data.Length <= 0 || !evt.Event.IsJson || evt.Event.EventType.StartsWith("$") || evt.IsResolved)
                    return Task.CompletedTask;
                var e = _deserializer(evt);
                if (e is IEvent message) {
                    Apply(message);
                }
                // ReSharper disable once PossibleInvalidOperationException
                // this always has a value here
                _checkPoint = evt.OriginalPosition.Value;
            }

            if (_isLive) { UpdateSubscribers(); }

            return Task.CompletedTask;
        }
       
        //IReadModel implementation
        public TModel Current {
            get {
                lock (_updateLock) {
                    return Model;
                }
            }
        }

        public void Subscribe(Action<TModel> target)
        {
            _subscribers.Add(target);
        }
        
        private void UpdateSubscribers()
        {
            if (!_isLive) {
                return;
            }
            lock (_updateLock) {
                var current = Model;
                foreach (var target in _subscribers) {
                    target(current);
                }
            }
        }
        //TODO: Implement rebuild from snapshot support
        public SnapShot<TModel> Snapshot => new SnapShot<TModel>(_checkPoint, Model);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                _subscription?.Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
