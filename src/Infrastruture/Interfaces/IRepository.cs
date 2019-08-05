using System;

namespace Infrastructure {
    public interface IRepository {
        void Save(IEventSource source);
        T Load<T>(Guid id) where T : class, IEventSource;
    }
}