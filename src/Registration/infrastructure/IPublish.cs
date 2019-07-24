namespace Registration.infrastructure
{
    public interface IPublish{
        void Publish(IMessage message);
    }
}