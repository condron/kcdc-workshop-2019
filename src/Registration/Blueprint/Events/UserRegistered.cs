using System;
using Infrastructure;

namespace Registration.Blueprint.Events{
    public class UserRegistered : IEvent
    {
        public readonly Guid UserId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public UserRegistered(Guid userId,
            string firstName,
            string lastName,
            string email)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }
}