using System;

namespace Infrastructure {
    public interface IReadModel<T>
    {
        Tuple<CheckPoint, T> GetCurrent();
        void SubscribeToChanges(Action<Tuple<CheckPoint, T>> target);
    }
}