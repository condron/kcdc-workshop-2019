using System;
using System.Collections.Generic;
using System.Reflection;

namespace Infrastruture
{
    public abstract class WriterBase:IEventSource
    {
        protected void Raise(IEvent @event)
        {
            _pendingEvents.Add(@event);
            ApplyOnParent(@event);
        }

        private void ApplyOnParent(IEvent @event)
        {
            var apply = (this as dynamic).GetType().GetMethod("Apply", BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] {@event.GetType()}, null);
            apply?.Invoke(this, new object[] {@event});
        }

        private List<IEvent> _pendingEvents = new List<IEvent>();
        private long _version = -1;
        protected Guid Id;

        long IEventSource.Version => _version;
        Guid IEventSource.Id => Id;

        string IEventSource.Name {
            get {
                dynamic derived = this;
                return derived.GetType().Name;
            }
        }

        void IEventSource.Hydrate(IEnumerable<IEvent> events)
        {
            //if created by repo complete object setup
            if(_pendingEvents == null) _pendingEvents = new List<IEvent>();
            _version = -1;

            foreach (var @event in events)
            {
                ApplyOnParent(@event);
                if (_version < 0) // new aggregates have a expected version of -1 or -2
                    _version = 0; // got first event (zero based)
                else
                    _version++;
            }
        }
        IReadOnlyList<IEvent> IEventSource.TakeEvents()
        {
            if (Id == Guid.Empty) {
                throw new InvalidOperationException("Writer must have ID set prior to Taking Events.");
            }

            var pending = new List<IEvent>(_pendingEvents);
            _pendingEvents.Clear();
            return pending;
        }
    }
}
