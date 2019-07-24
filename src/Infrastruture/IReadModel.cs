using System;

namespace Infrastructure {
    public interface IReadModel<T>
    {
        Tuple<CheckPoint, T> GetCurrent(Action<Tuple<CheckPoint, T>> target = null);
        void SubscribeToChanges(Action<Tuple<CheckPoint, T>> target);
    }
}