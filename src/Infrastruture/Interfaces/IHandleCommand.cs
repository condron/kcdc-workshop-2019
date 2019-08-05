namespace Infrastructure{
    public interface IHandleCommand<in TCommand> where TCommand : ICommand
    {
        bool Handle(TCommand cmd);
    }
}