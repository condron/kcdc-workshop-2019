using System;
using Registration.Blueprint.Events;
using Registration.infrastructure;

namespace Registration.EventWriters
{
    internal class User : 
        WriterBase
        
    {
        internal User(
            Guid id,
            string firstName,
            string lastname,
            string email)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastname)) {
                throw new Exception("User must have a name");
            }
            if (string.IsNullOrEmpty(email)) {
                throw new Exception("User must have an email");
            }
            if (id == Guid.Empty) {
                throw new Exception("User must have a non empty id");
            }

            Raise(new UserRegistered(id, firstName, lastname, email));
        }
        public void ChangeName(string first, string last)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last)) {
                throw new Exception("User must have a name");
            }

            Raise(new NameChanged(Id, first, last));
        }
        //state changes
        private void Apply(UserRegistered @event)
        {
            Id = @event.UserId;
        }

        private void Apply(NameChanged @event)
        {
            // nothing to do
        }
    }
}