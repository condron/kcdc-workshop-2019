using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.infrastructure
{
    public interface IApply<in TEvent> where TEvent : IEvent
    {
        void Apply(TEvent @event);
    }
}
