using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Infrastructure
{
    public abstract class Reader<TModel> : 
        IReadModel<TModel> where TModel : class, new()
    {
        private readonly object _updateLock = new object();
        private readonly IEventStoreConnection _conn;
        private readonly Func<ResolvedEvent, object> _deserializer;
        private EventStoreAllCatchUpSubscription _subscription;

        protected TModel Model;
        private Position _checkPoint;

        private readonly List<Action<TModel>> _targets = new List<Action<TModel>>();

        protected Reader(
            IEventStoreConnection conn,
            Func<ResolvedEvent, object> deserializer,
            SnapShot<TModel> snapshot = null)
        {
            _conn = conn;
            _deserializer = deserializer;
            _checkPoint = snapshot?.At ?? Position.Start;
            Model = snapshot?.Data ?? new TModel();
        }

        private bool _isLive = false;

        public void Start()
        {
            _subscription = _conn.SubscribeToAllFrom(
                Position.Start,
                CatchUpSubscriptionSettings.Default,
                GotEvent,
                (_) => { _isLive = true; }
            );
            SpinWait.SpinUntil(() => _isLive);
            UpdateTargets();
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
                _checkPoint = evt.OriginalPosition.Value;
            }

            if (_isLive) { UpdateTargets(); }

            return Task.CompletedTask;
        }
        private void Apply(IEvent @event)
        {
            var apply = (this as dynamic).GetType().GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance, null, new[] { @event.GetType() }, null);
            apply?.Invoke(this, new object[] { @event });

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
            _targets.Add(target);
        }

        public SnapShot<TModel> Snapshot => new SnapShot<TModel>(_checkPoint, Model);

        private void UpdateTargets()
        {
            if (!_isLive) {
                return;
            }
            lock (_updateLock) {
                var current = Model;
                foreach (var target in _targets) {
                    target(current);
                }
            }
        }

    }
}
