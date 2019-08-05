using System;

namespace Infrastructure {
    public interface IReadModel<T>
    {
        T Current { get; }
        SnapShot<T> Snapshot { get; }
        void Subscribe(Action<T> target);
    }
}