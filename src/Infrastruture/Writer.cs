using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public abstract class Writer : EventDrivenStateMachine, IEventSource
    {
        private List<IEvent> _pendingEvents = new List<IEvent>();
        protected void Raise(IEvent @event)
        {
            _pendingEvents.Add(@event);
            Apply(@event);
        }


        //IEventSource
        private long _version = -1;
        long IEventSource.Version => _version;

        protected Guid Id;
        Guid IEventSource.Id => Id;

        string IEventSource.Name {
            get {
                dynamic derived = this;
                return derived.GetType().Name;
            }
        }
        //TODO: Implement hot writer and snapshot support
        void IEventSource.Hydrate(IEnumerable<IEvent> events)
        {
            //created by reflection so complete object setup
            if (_pendingEvents == null) _pendingEvents = new List<IEvent>();
            _version = -1;

            foreach (var @event in events) {

                Apply(@event);
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
