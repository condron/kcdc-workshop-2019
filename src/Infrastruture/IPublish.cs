namespace Infrastructure
{
    public interface IPublish{
        void Publish(IMessage message);
    }
}