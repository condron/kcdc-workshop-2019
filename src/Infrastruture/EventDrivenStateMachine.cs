using System.Reflection;


namespace Infrastructure
{
    public abstract class EventDrivenStateMachine
    {
        protected void Apply(IEvent @event)
        {
            var apply = (this as dynamic).GetType().GetMethod("Apply", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { @event.GetType() }, null);
            apply?.Invoke(this, new object[] { @event });

        }
    }
}
