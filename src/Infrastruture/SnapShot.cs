using EventStore.ClientAPI;

namespace Infrastructure
{
    public class SnapShot<T>
    {
        public SnapShot(Position at, T data)
        {
            At = at;
            Data = data;
        }
        public Position At { get; }
        public T Data { get; }
    }
}
