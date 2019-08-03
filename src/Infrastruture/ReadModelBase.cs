using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Infrastructure
{
    public abstract class ReadModelBase<TModel> : IReadModel<TModel> where TModel : class, new()
    {
        private object _updateLock = new object();
        private readonly IEventStoreConnection _conn;
        private readonly Func<ResolvedEvent, object> _deserializer;
        private EventStoreAllCatchUpSubscription _subscription;

        protected TModel Model;
        private CheckPoint _checkPoint;

        private readonly List<Action<Tuple<CheckPoint, TModel>>> _targets =
            new List<Action<Tuple<CheckPoint, TModel>>>();

        protected ReadModelBase(
            IEventStoreConnection conn,
            Func<ResolvedEvent, object> deserializer,
            CheckPoint checkpoint = null,
            TModel snapshot = null)
        {
            _conn = conn;
            _deserializer = deserializer;
            _checkPoint = checkpoint ?? new AllCheckpoint(Position.Start);
            Model = snapshot ?? new TModel();
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
                   ApplyOnParent(message);
                }
                _checkPoint = new AllCheckpoint(evt.OriginalPosition.Value);
            }

            if (_isLive) { UpdateTargets();}

            return Task.CompletedTask;
        }
        private void ApplyOnParent(IEvent @event)
        {
            var apply = (this as dynamic).GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance, null,
                new[] { @event.GetType() }, null);
            apply?.Invoke(this, new object[] { @event });
            
        }
        public Tuple<CheckPoint, TModel> GetCurrent()
        {
            lock (_updateLock) {
                return new Tuple<CheckPoint, TModel>(_checkPoint, Model);
            }
        }

        public void SubscribeToChanges(Action<Tuple<CheckPoint, TModel>> target)
        {
            _targets.Add(target);
        }
        private void UpdateTargets()
        {
            if (!_isLive) {
                return;
            }
            lock (_updateLock) {
                var current = Model;
                var checkPoint = _checkPoint;
                foreach (var target in _targets) {
                    target(new Tuple<CheckPoint, TModel>(checkPoint, current));
                }
            }
        }

    }
}
