using System;
using Infrastructure;

namespace Registration.Blueprint.Events{
    public class NameChanged : IEvent
    {
        public readonly Guid UserId;
        public readonly string FirstName;
        public readonly string LastName;

        public NameChanged(Guid userId,
            string firstName,
            string lastName)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}