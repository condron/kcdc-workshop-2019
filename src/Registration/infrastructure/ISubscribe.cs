using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.infrastructure
{
    public interface ISubscribe{
        void Subscribe<TMessage>(IHandle<TMessage> subscriber) where TMessage : IEvent;
        void Subscribe<TMessage>(IHandleCommand<TMessage> subscriber) where TMessage : ICommand;
    }
}
