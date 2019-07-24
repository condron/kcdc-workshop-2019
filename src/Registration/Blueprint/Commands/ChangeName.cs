using System;
using Infrastructure;

namespace Registration.Blueprint.Commands{
    public class ChangeName : IEvent, ICommand{
        public readonly Guid UserId;
        public readonly string FirstName;
        public readonly string LastName;

        public ChangeName(Guid userId,
            string firstName,
            string lastName)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}