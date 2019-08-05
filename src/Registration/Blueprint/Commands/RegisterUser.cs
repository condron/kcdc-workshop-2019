using System;
using Infrastructure;

namespace Registration.Blueprint.Commands{
    public class RegisterUser : ICommand
    {
        public readonly Guid UserId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public RegisterUser(Guid userId,
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