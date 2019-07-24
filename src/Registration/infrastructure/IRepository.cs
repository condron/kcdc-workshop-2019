using System;

namespace Registration.infrastructure {
    public interface IRepository {
        void Save(IEventSource source);
        T Load<T>(Guid id) where T : class, IEventSource;
    }
}