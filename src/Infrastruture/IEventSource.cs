using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public interface IEventSource
    {
        string Name { get; }
        Guid Id { get; }
        long Version{ get; }
        void Hydrate(IEnumerable<IEvent> events);
        IReadOnlyList<IEvent> TakeEvents();
    }
}
