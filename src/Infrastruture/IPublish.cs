namespace Infrastruture
{
    public interface IPublish{
        void Publish(IMessage message);
    }
}