using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public class SimpleBus:IBus
    {
        private readonly Dictionary<Type,List<Action<IMessage>>> _handles = new Dictionary<Type, List<Action<IMessage>>>();
        public void Publish(IMessage message){
            var mType = message.GetType();
            if (!_handles.ContainsKey(mType)) return;
            foreach (var action in _handles[mType]){
                action(message);
            }
        }

        public void Subscribe<TMessage>(IHandle<TMessage> subscriber) where TMessage : IEvent{
            var mType = typeof(TMessage);
            if (!_handles.ContainsKey(mType)){
                _handles.Add(mType,new List<Action<IMessage>>());
            }
            _handles[mType].Add( msg => subscriber.Handle((TMessage)msg));
        }

        public void Subscribe<TMessage>(IHandleCommand<TMessage> subscriber) where TMessage : ICommand{
            var mType = typeof(TMessage);
            if (!_handles.ContainsKey(mType)){
                _handles.Add(mType,new List<Action<IMessage>>());
            }
            _handles[mType].Add( msg => subscriber.Handle((TMessage)msg));
        }
    }
}
